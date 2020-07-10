/* HOMYOKO LOUVERS CONTROLLER
 * 
 * Umožňuje ovládanie žalúzí, ktoré vykraftil foko.
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 */ 
using System;
using System.Collections.Generic;
using System.Text;

namespace X.Homylogic.Models.Objects.Devices.Homyoko
{
    class Louvers : TCPDeviceX
    {
        const string TITLE = "Louvers";





        public Louvers() 
        {
            // base.DeviceType = DeviceTypes.HomyokoLouvers;
            this._ignoreLogDisconnect = true;
        }




    }
}
