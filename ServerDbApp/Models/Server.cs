using System;
using System.Collections.Generic;
using System.Text;

namespace ServerDbApp.Models;

public class Server
{
    public int Id { get; set; }

    public string Name { get; set; }

    public Server(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public Server() : this(0, string.Empty)
    {
    }

    public override string ToString()
    {
        return $"[{Id}] {Name}";
    }
}