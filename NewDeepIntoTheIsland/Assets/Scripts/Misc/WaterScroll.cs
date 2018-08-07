using UnityEngine;
using System.Collections;

public class WaterScroll : MonoBehaviour
{
    //Water texture offset scroll speed in X and Y Axis
    public float scrollSpeed = -0.07f;

    //Water model renderer for access to material instance
    Renderer WaterRenderer;

    //name of the shader properties to scroll uv offsets
    public string nameParameter = "_MainTex";

    float offsetRandom;

    void Start()
    {
        if (!WaterRenderer)
            WaterRenderer = GetComponent<Renderer>();
        offsetRandom = Random.Range(0f, 1f);
    }

    void Update()
    {
        float offset1 = offsetRandom + Time.time * scrollSpeed;

        //Property name1 = > offset1
        WaterRenderer.material.SetTextureOffset(nameParameter, new Vector2(offset1, 0));
        WaterRenderer.material.SetTextureOffset("_HeightMap", new Vector2(offset1, 0));
        WaterRenderer.material.SetFloat("_HeightOffset", 0f);
    }
    /*
        //Water texture offset scroll speed in X and Y Axis
        public float scrollSpeed1 = -0.07f, scrollSpeed2 = -0.07f;

        //Water model renderer for access to material instance
        public Renderer WaterRenderer;

        //name of the shader properties to scroll uv offsets
        public string name1 = "_MainTex", name2 = "_SpecTex";

        void Start ()
        {
            if (!WaterRenderer)
                WaterRenderer = GetComponent<Renderer> ();
        }

        void Update ()
        {
            float offset1 = Time.time * scrollSpeed1;
            float offset2 = Time.time * scrollSpeed2;

            //Property name1 = > offset1
            WaterRenderer.material.SetTextureOffset (name1, new Vector2 (offset1, 0));
            //   -   Property name2 = > offset2
            WaterRenderer.material.SetTextureOffset (name2, new Vector2 (offset2, 0));
        }
        */
}
