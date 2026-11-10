using Microsoft.EntityFrameworkCore;
using System.IO;
using System;

namespace Ktpt.CourseOrder.Data;

public static class Db
{
    public static BoardGamesContext Create()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "connection.txt");
        string connection = File.ReadAllText(path).Trim();
        var options = new DbContextOptionsBuilder<BoardGamesContext>()
            .UseSqlServer(connection)
            .Options;
        return new BoardGamesContext(options);
    }
}