using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FrogCore;
using Satchel;
using UnityEngine;

namespace CorruptedDreamsPlando.FsmEditTemplates
{
    public class SpriteSwapper
    {
        private static tk2dSpriteAnimation _spellAnimation;
        private static tk2dSpriteCollectionData _spellCollection;

        public static void SetupSpellTextureSwap(tk2dSprite spellObject)
        {
            var animator = spellObject.gameObject.GetComponent<tk2dSpriteAnimator>();

            if (_spellAnimation == null || _spellCollection == null)
            {
                Texture2D spellTexture = new Texture2D(2, 2);
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CorruptedDreamsPlando.Resources.spellAnimations.GrimmSpellSprites.png"))
                {
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);
                    spellTexture.LoadImage(bytes, true);
                    spellTexture.name = "PeekaIsAwesome";
                }

                //_spellCollection = animator.DefaultClip.frames[0].spriteCollection.CloneTk2dCollectionWithTexture(spellTexture, "PeekaIsAwesome");

                tk2dSpriteAnimationClip clip = Array.Find(animator.Library.clips, c => c.name == "Ball Lv2");
                _spellCollection = clip.frames[0].spriteCollection.CloneTk2dCollectionWithTexture(spellTexture, "PeekaIsAwesome");
                _spellAnimation = animator.Library.CloneTk2dAnimationWithCollection(_spellCollection, "PeekaIsAwesome");
            }

            animator.Library = _spellAnimation;
            spellObject.SetSprite(_spellCollection, spellObject.spriteId);
            spellObject.SpriteChanged += OnSpriteChange;
        }

        private static void OnSpriteChange(tk2dBaseSprite sprite)
        {
            sprite.SetSprite(_spellCollection, sprite.spriteId);
        }
    }
}
