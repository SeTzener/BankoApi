namespace BankoApi.Data.Dao;

using System;
using System.Collections.Generic;

public class Requisition
{
    public string Id { get; set; }
    public DateTime Created { get; set; }
    public string Redirect { get; set; }
    public string Status { get; set; }
    public string InstitutionId { get; set; }
    public string Agreement { get; set; }
    public string Reference { get; set; }
    public List<string> Accounts { get; set; }
    public string UserLanguage { get; set; }
    public string Link { get; set; }
    public string SSN { get; set; }
    public bool AccountSelection { get; set; }
    public bool RedirectImmediate { get; set; }
}
