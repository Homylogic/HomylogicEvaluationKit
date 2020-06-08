using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using X.Data.Factory;
using X.Homylogic;
using X.Homylogic.Models;
using X.Homylogic.Models.Objects.Buffers;

namespace HomylogicAsp.Models
{
    public class BuffersViewModel
    {
        public InputBufferXList InputBufferList => Body.Runtime.InputBuffers;
        public OutputBufferXList OutputBufferList => Body.Runtime.OutputBuffers;
    }
}
