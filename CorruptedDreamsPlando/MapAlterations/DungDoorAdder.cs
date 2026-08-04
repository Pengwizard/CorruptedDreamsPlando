using ItemChanger.Locations;
using MoreDoors.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorruptedDreamsPlando.MapAlterations
{
    public class DungDoorAdder
    {
        public static void AddDungDoor()
        {
            if (DoorData.GetDoor("Dung Door") != null)
            {
                return;
            }

            DoorData dungDoor = new DoorData();
            dungDoor.UIName = "Dung Door";
            dungDoor.CamelCaseName = "DungDoor";
            dungDoor.UpperCaseName = "DUNG_DOOR";


            var doorInfo = new MoreDoors.Data.DoorData.DoorInfo();
            doorInfo.Sprite = new EmbeddedSprite("DungDoor");
            doorInfo.LeftLocation = new DoorData.DoorInfo.Location();
            doorInfo.LeftLocation.SceneName = "Waterways_05";
            doorInfo.LeftLocation.GateName = "bot1";
            doorInfo.LeftLocation.X = 39;
            doorInfo.LeftLocation.Y = 13.4f;
            doorInfo.LeftLocation.NoKeyDesc = "A once-elegant dark pink door, worn from exposure to the city's waste, with a sphere-shaped lock.";
            doorInfo.LeftLocation.KeyDesc = "A once-elegant dark pink, worn from exposure to the city's waste, matching that spherical key.<br>Insert the Dung Key?";
            doorInfo.LeftLocation.RequiresLantern = false;
            doorInfo.LeftLocation.FadeOutOnOpen = false;
            doorInfo.RightLocation = new DoorData.DoorInfo.Location();
            doorInfo.RightLocation.SceneName = "Waterways_05";
            doorInfo.RightLocation.GateName = "bot1";
            doorInfo.RightLocation.X = 35;
            doorInfo.RightLocation.Y = 13.4f;
            doorInfo.RightLocation.NoKeyDesc = "A once-elegant dark pink door, worn from exposure to the city's waste, with a sphere-shaped lock.";
            doorInfo.RightLocation.KeyDesc = "A once-elegant dark pink, worn from exposure to the city's waste, matching that spherical key.<br>Insert the Dung Key?";
            doorInfo.RightLocation.RequiresLantern = false;
            doorInfo.RightLocation.FadeOutOnOpen = false;
            doorInfo.Mode = DoorData.DoorInfo.SplitMode.LeftTwin;

            var keyInfo = new DoorData.KeyInfo();
            keyInfo.ItemName = "DungKey";
            keyInfo.UIItemName = "Dung Key";
            keyInfo.Sprite = new EmbeddedSprite("DungKey");
            keyInfo.InvDesc = "A stained, sphere-like key. Wiping away the stays reveals a hint of dark pink.";
            keyInfo.UsedInvDesc = "This once-regal key gave you access to one of the last great knights.";
            keyInfo.Location = new CoordinateLocation()
            {
                name = "DungKey",
                sceneName = "Waterways_01",
                x = 33f,
                y = 11.4f,
            };

            keyInfo.WorldMapLocationOverride = new DoorData.KeyInfo.WorldMapLocation()
            {
                SceneName = keyInfo.Location.sceneName,
                X = 33f,
                Y = 11.4f,
            };

            dungDoor.Door = doorInfo;
            dungDoor.Key = keyInfo;
            MoreDoors.Data.DoorData.AddExtensionDoor("DungDoor", dungDoor);
        }
    }
}
