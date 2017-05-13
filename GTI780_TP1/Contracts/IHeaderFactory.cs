using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GTI780_TP1.Contracts.Entities;

namespace GTI780_TP1.Contracts
{
    /// <summary>
    /// Defines the behaviors of a HeaderFactory
    /// </summary>
    public interface IHeaderFactory
    {

        Header Create(HeaderType headerType);
    }
}
