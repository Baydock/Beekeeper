using UnhollowerBaseLib;
using UnityEngine;

namespace Beekeeper {
    internal class Tower2DAnimator : MonoBehaviour {
        private float timeToWait;
        private float highlightTimeToWait;

        private float currentTime = 0;
        private int currentFrame = 0;

        public string framesId;
        public string highlightFramesId;
        // not normal arrays so that no conversions are necessary for UpdateSprite
        private Il2CppReferenceArray<Sprite> frames;
        private Il2CppReferenceArray<Sprite> highlightFrames;
        private SpriteRenderer spriteRenderer;

        public bool Highlighted = false;

        public Tower2DAnimator(System.IntPtr ptr) : base(ptr) { }

        private void GetFrames() {
            byte[][] bytes = Beekeeper.LoadFrames(framesId, out float ppu, out float fps);

            timeToWait = 1 / fps;

            frames = new Sprite[bytes.Length];
            for (int i = 0; i < frames.Length; i++)
                frames[i] = Mod.CreateSprite(Mod.LoadTexture(bytes[i]), ppu);
        }

        private void GetHighlightFrames() {
            byte[][] bytes = Beekeeper.LoadFrames(highlightFramesId, out float ppu, out float fps);

            highlightTimeToWait = 1 / fps;

            highlightFrames = new Sprite[bytes.Length];
            for (int i = 0; i < highlightFrames.Length; i++)
                highlightFrames[i] = Mod.CreateSprite(Mod.LoadTexture(bytes[i]), ppu);
        }

        // anything but basic types, such as strings or ints, are not carried over to the Il2Cpp side, so they must always be asigned there
        public void Start() {
            spriteRenderer = GetComponent<SpriteRenderer>();

            GetFrames();
            GetHighlightFrames();

            spriteRenderer.sprite = frames[0];
        }

        public void Update() {
            if (enabled) {
                currentTime += Time.deltaTime;

                if (Highlighted) {
                    if (frames is null) {
                        if (framesId is null)
                            return;
                        GetFrames();
                    }
                    UpdateSprite(highlightTimeToWait, highlightFrames, new System.Action(GetHighlightFrames));
                } else {
                    if (highlightFrames is null) {
                        if (highlightFramesId is null)
                            return;
                        GetHighlightFrames();
                    }
                    UpdateSprite(timeToWait, frames, new System.Action(GetFrames));
                }
            }
        }

        // Needs to be il2cpp types to be able to be in il2cpp derived class, otherwise MelonStinker makes a warning even though it works
        private void UpdateSprite(float timeToWait, Il2CppReferenceArray<Sprite> frames, Il2CppSystem.Action getFrames) {
            if (currentTime > timeToWait) {
                currentTime -= timeToWait;
                currentFrame = (currentFrame + 1) % frames.Length;
                if (frames[currentFrame] == null)
                    getFrames.Invoke();
                spriteRenderer.sprite = frames[currentFrame];
            }
        }
    }
}
