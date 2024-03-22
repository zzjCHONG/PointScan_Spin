using System.Diagnostics;
using Simscop.API.TopLaser;

namespace Simscop.API
{
    public class TopticaLaser : ILaser
    {
        Toptica topticaLaser = new();

        public string GetConnectState() => topticaLaser.GetConnectState();

        public bool GetPower(int count, out int value) => topticaLaser.GetChannelPower(count, out value);
      
        public bool Init() => topticaLaser.Connect();

        public bool SetPower(int count, int value) => topticaLaser.SetChannelPower(count, value);

        public bool SetStatus(int count, bool status)=>topticaLaser.SetLaserState(count, status);

        public bool GetStatus(int count, out bool status)=>topticaLaser.GetStatus(count, out status);

        public bool DisConnect()=>topticaLaser.Disconnect();

    }

}
