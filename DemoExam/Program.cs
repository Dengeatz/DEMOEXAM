using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;

namespace DemoExam
{
    public class Program
    {


        public static void Main(string[] args)
        {
        
            List<Order> repo = new List<Order>() { new Order(0, new(2024,2,2), "dsadas", "model", "desc", "fullname", "+7952", "Не начато"),
            new Order(1, new(2023,2,1), "type1", "model1", "desc1", "fullname1", "+7812", "В процессе ремонта")};
            
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors();
            var app = builder.Build();
            app.UseCors(o => o.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            string message = "";

            app.MapGet("orders", (int param = 0) =>
            {
                string buffer = message;
                message = "";
                if (param != 0)
                {
                    return new { repo = repo.FindAll(x => x.Number == param), message = buffer };
                }
                return new { repo, message = buffer };
            });

            app.MapGet("create", ([AsParameters] Order dto) => repo.Add(dto));

            app.MapGet("update", ([AsParameters] OrderUpdateDTO dto) =>
            {
                var order = repo.Find((o) => o.Number == dto.number);

                if (order == null)
                    return;

                if (dto.status != order.Status && dto.status != "")
                {
                    order.Status = dto.status;
                    message += $"Статус заявки {order.Number} изменен\n";
                    if (order.Status == "Готова к выдаче")
                    {
                        message += $"Заявка {order.Number} готова к выдаче\n";
                        order.EndDate = DateOnly.FromDateTime(DateTime.Now);
                    }
                }
                if (dto.desc != "")
                    order.Desc = dto.desc;
                if (dto.master != "")
                    order.Master = dto.master;
                if (dto.comment != "")
                {
                    order.Comments?.Add(dto.comment);
                }
            });

            int complete_count() => repo.FindAll(x => x.Status == "Готова к выдаче").Count;

            Dictionary<string, int> get_problem_type_stat() =>
                repo.GroupBy(x => x.Desc)
                .Select(x => (x.Key, x.Count()))
                .ToDictionary(k => k.Key, v => v.Item2);

            double get_average_time_to_complete() =>
                complete_count() == 0 ? 0 :
                repo.FindAll(x => x.Status == "Готова к выдаче" && x.EndDate != null)
                .Select(x => x.EndDate.Value.DayNumber - x.StartDate.DayNumber)
                .Sum() / complete_count();

            app.MapGet("/statistics", () => new {
                complete_count = complete_count(),
                problem_type_stat = get_problem_type_stat(),
                average_time_to_complete = get_average_time_to_complete()
            });

            app.Run();
        }

        class Order(int number, DateOnly startDate, string type, string model, string desc, string fullname, string phone, string status)
        {
            public int Number { get; set; } = number;
            public DateOnly StartDate { get; set; } = startDate;
            public string Type { get; set; } = type;
            public string Model { get; set; } = model;
            public string Desc { get; set; } = desc;
            public string FullName { get; set; } = fullname;
            public string Phone { get; set; } = phone;
            public string Status { get; set; } = status;

            public string? Master { get; set; } = "Не назначен";

            public DateOnly? EndDate { get; set; } = null;

            public List<string>? Comments { get; set; } = [];

        }

        record class OrderUpdateDTO(int number, string? status, string? desc, string? master, string? comment);
    }
}
