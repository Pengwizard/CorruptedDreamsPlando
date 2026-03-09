using ItemChanger;
using ItemChanger.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CorruptedDreamsPlando
{
    [Serializable]
    public class EmbeddedSprite : ISprite
    {
        private static SpriteManager EmbeddedSpriteManager = new(typeof(EmbeddedSprite).Assembly, "CorruptedDreamsPlando.Resources.");

        public string key;
        public EmbeddedSprite(string key)
        {
            this.key = key;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Sprite Value => EmbeddedSpriteManager.GetSprite(key);
        public ISprite Clone() => (ISprite)MemberwiseClone();
    }
}
