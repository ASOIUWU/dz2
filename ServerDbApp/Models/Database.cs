using System;
using System.Collections.Generic;
using System.Text;

namespace ServerDbApp.Models;

public class Database
{
    public int Id { get; set; }

    public int ServerId { get; set; }

    public string Name { get; set; }

    private int _sizeGb;

    public int SizeGb
    {
        get => _sizeGb;
        set
        {
            if (value < 0)
                throw new ArgumentException("Объём данных не может быть отрицательным");
            _sizeGb = value;
        }
    }

    public Database(int id, int serverId, string name, int sizeGb)
    {
        Id = id;
        ServerId = serverId;
        Name = name;
        SizeGb = sizeGb;
    }

    public Database() : this(0, 0, string.Empty, 0)
    {
    }

    public override string ToString()
    {
        return $"[{Id}] {Name}, сервер #{ServerId}, объём: {SizeGb} ГБ";
    }
}