using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using X.Data.Factory;
using X.Homylogic;
using X.Homylogic.Models.Objects;
using X.App.Logs;

namespace HomylogicAsp.Models
{
    public class LogsViewModel
    {
        public LogList LogList => Body.Environment.Logs;
    }
}
