using UnityEngine;

namespace OrbitBreaker
{
    public sealed class ParallaxBackground : MonoBehaviour
    {
        public GameAssets assets;
        public GameController game;
        private readonly Transform[,] tiles = new Transform[2, 2];
        private readonly float[] offsets = new float[2];
        private const float TileHeight = 20f;

        private void Start()
        {
            for (int layer = 0; layer < 2; layer++)
                for (int tile = 0; tile < 2; tile++)
                {
                    var go = new GameObject("Stars " + layer + ":" + tile);
                    go.transform.SetParent(transform, false);
                    go.transform.localScale = Vector3.one * 5f; // 512 px / 128 PPU * 5 = 20 world units.
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = assets.stars[layer];
                    sr.sortingOrder = -20 + layer;
                    sr.color = layer == 0 ? new Color(.35f, .5f, .8f, .6f) : new Color(.75f, .9f, 1f, .8f);
                    tiles[layer, tile] = go.transform;
                }
            PositionTiles();
        }

        private void Update()
        {
            if (game.Phase == GamePhase.Paused || game.Phase == GamePhase.GameOver) return;
            for (int layer = 0; layer < 2; layer++)
                offsets[layer] = Mathf.Repeat(offsets[layer] + Time.deltaTime * (layer == 0 ? .18f : .55f), TileHeight);
            PositionTiles();
        }

        private void PositionTiles()
        {
            for (int layer = 0; layer < 2; layer++)
                for (int tile = 0; tile < 2; tile++)
                    tiles[layer, tile].localPosition = new Vector3(layer == 0 ? -.35f : .35f,
                        TileHeight * tile - offsets[layer], 0f);
        }
    }
}
