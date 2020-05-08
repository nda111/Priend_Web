using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriendWeb.Data
{
    /// <summary>
    /// An enumeration that represents sex of animal including weather neutered or not
    /// </summary>
    public enum Sex : short
    {
        Male = 0b01,
        Female = 0b00,
        Neutered = 0b11,
        Spayed = 0b10
    }
}
