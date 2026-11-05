using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

string connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Не задана строка подключения.");

SqlConnection Open()
{
    var connection = new SqlConnection(connectionString);
    connection.Open();
    return connection;
}

List<Dictionary<string, object?>> Query(string sql, params SqlParameter[] parameters)
{
    var result = new List<Dictionary<string, object?>>();
    using var connection = Open();
    using var command = new SqlCommand(sql, connection);
    command.Parameters.AddRange(parameters);
    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
        var row = new Dictionary<string, object?>();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
        }
        result.Add(row);
    }
    return result;
}

object? Scalar(string sql, params SqlParameter[] parameters)
{
    using var connection = Open();
    using var command = new SqlCommand(sql, connection);
    command.Parameters.AddRange(parameters);
    return command.ExecuteScalar();
}

const string ToursSelect = @"
SELECT t.Kod_tura AS id, t.Nazvanie AS nazvanie, s.Strana AS strana, g.Gorod AS gorod,
       t.Data_nachala AS dataNachala, t.Data_okonchaniya AS dataOkonchaniya,
       t.Stoimost AS stoimost, t.Kolichestvo_mest AS kolichestvoMest,
       t.Kolichestvo_mest - ISNULL((SELECT COUNT(*) FROM KURS_Zayavka z
                                    WHERE z.Kod_tura = t.Kod_tura AND z.Kod_statusa <> 4), 0) AS svobodno
FROM KURS_Tur t
JOIN KURS_Gorod g ON g.Kod_goroda = t.Kod_goroda
JOIN KURS_Strana s ON s.Kod_strany = g.Kod_strany";

app.MapGet("/api/tours", (string? country, string? dateFrom, string? dateTo, decimal? maxPrice) =>
{
    var sql = ToursSelect + " WHERE 1 = 1";
    var parameters = new List<SqlParameter>();
    if (!string.IsNullOrEmpty(country))
    {
        sql += " AND s.Strana = @country";
        parameters.Add(new SqlParameter("@country", country));
    }
    if (DateTime.TryParse(dateFrom, out var from))
    {
        sql += " AND t.Data_nachala >= @from";
        parameters.Add(new SqlParameter("@from", from));
    }
    if (DateTime.TryParse(dateTo, out var to))
    {
        sql += " AND t.Data_nachala <= @to";
        parameters.Add(new SqlParameter("@to", to));
    }
    if (maxPrice.HasValue)
    {
        sql += " AND t.Stoimost <= @maxPrice";
        parameters.Add(new SqlParameter("@maxPrice", maxPrice.Value));
    }
    sql += " ORDER BY t.Data_nachala";
    return Results.Ok(Query(sql, parameters.ToArray()));
});

app.MapGet("/api/tours/{id:int}", (int id) =>
{
    var rows = Query(ToursSelect + " WHERE t.Kod_tura = @id", new SqlParameter("@id", id));
    return rows.Count == 0 ? Results.NotFound(new { message = "Тур не найден" }) : Results.Ok(rows[0]);
});

app.MapPost("/api/requests", (RequestDto request) =>
{
    if (request.klientId <= 0 || request.turId <= 0 || request.sotrudnikId <= 0)
    {
        return Results.BadRequest(new { message = "Укажите клиента, тур и сотрудника" });
    }

    var svobodno = Convert.ToInt32(Scalar(
        "SELECT Kolichestvo_mest - ISNULL((SELECT COUNT(*) FROM KURS_Zayavka z WHERE z.Kod_tura = @id AND z.Kod_statusa <> 4), 0) FROM KURS_Tur WHERE Kod_tura = @id",
        new SqlParameter("@id", request.turId)) ?? -1);

    if (svobodno <= 0)
    {
        return Results.Conflict(new { message = "На выбранный тур нет свободных мест" });
    }

    var price = Convert.ToDecimal(Scalar("SELECT Stoimost FROM KURS_Tur WHERE Kod_tura = @id",
        new SqlParameter("@id", request.turId)));

    int newId;
    using (var connection = Open())
    using (var command = new SqlCommand(
        "INSERT INTO KURS_Zayavka (Kod_klienta, Kod_tura, Kod_sotrudnika, Kod_statusa, Data_podachi, Summa) " +
        "VALUES (@client, @tour, @sotrudnik, 1, CAST(GETDATE() AS date), @sum); SELECT SCOPE_IDENTITY();", connection))
    {
        command.Parameters.AddWithValue("@client", request.klientId);
        command.Parameters.AddWithValue("@tour", request.turId);
        command.Parameters.AddWithValue("@sotrudnik", request.sotrudnikId);
        command.Parameters.AddWithValue("@sum", price);
        newId = Convert.ToInt32(command.ExecuteScalar());
    }

    return Results.Created($"/api/requests/{newId}/status", new { id = newId, status = "Новая" });
});

app.MapGet("/api/requests/{id:int}/status", (int id) =>
{
    var rows = Query(
        "SELECT z.Kod_zayavki AS id, st.Status AS status FROM KURS_Zayavka z " +
        "JOIN KURS_Status st ON st.Kod_statusa = z.Kod_statusa WHERE z.Kod_zayavki = @id",
        new SqlParameter("@id", id));
    return rows.Count == 0 ? Results.NotFound(new { message = "Заявка не найдена" }) : Results.Ok(rows[0]);
});

app.Run();

record RequestDto(int klientId, int turId, int sotrudnikId);
