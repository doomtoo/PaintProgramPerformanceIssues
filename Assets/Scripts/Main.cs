using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[ExecuteInEditMode]
public class Main : MonoBehaviour
{
    public static int MAX_MATERIAL_VECTOR_SIZE_ACTUAL = 200;//android says max 240- but it goes over constantly set to 240

    public RawImage image;
    RenderTexture rendertex;
    Material material;

    Rect rect_image_BL;

    Color color=new Color(.5f, 1, .5f, 1);

    float aspect_ratio=1;
    float brush_size=250;//brush size in pixels
    float brush_size_UV_half_x=.1f;//brush size in percent UV of texture
    float brush_size_UV_half_y=.1f;//brush size in percent UV of texture

    Vector2 mouse_or_touch_point = new Vector2();
    List<float> mouse_points_x = new List<float>();
    List<float> mouse_points_y = new List<float>();

    bool max_points_set=false;

    void Start()
    {
        CenterImage();
        //Get Rect position from bottom left of scren, and pixel size, for converting mouse coords from screen to texture coords
        rect_image_BL = RectTransformUtility.PixelAdjustRect(image.rectTransform, image.transform.root.GetComponent<Canvas>()); 
        rect_image_BL.x=image.rectTransform.offsetMin.x;
        rect_image_BL.y=image.rectTransform.offsetMin.y;

        rendertex = (RenderTexture)image.texture;

        aspect_ratio = rendertex.width/rendertex.height;
        brush_size_UV_half_x= brush_size/((float)rendertex.width);
        brush_size_UV_half_y= brush_size/((float)rendertex.height);

        material = new Material(Shader.Find("Custom/BrushShader"));
        material.SetTexture("_MainTex", this.rendertex);     
        material.SetColor("_BrushColor", color);

        //brush size
        material.SetFloat("_Brush_size_UV_half_x", brush_size_UV_half_x);
        material.SetFloat("_Brush_size_UV_half_y", brush_size_UV_half_y);
        InitializeShaderArray();

        image.material=material;
    }
    private void CenterImage()
    {
        if(Screen.width>Screen.height)        
            image.rectTransform.sizeDelta= new Vector2(Screen.height, Screen.height);                    
        else        
            image.rectTransform.sizeDelta= new Vector2(Screen.width, Screen.width);
        
        image.rectTransform.offsetMin= new Vector2(Screen.width*.5f-image.rectTransform.sizeDelta.x*.5f, Screen.height*.5f-image.rectTransform.sizeDelta.y*.5f );
        image.rectTransform.offsetMax= new Vector2(Screen.width*.5f+image.rectTransform.sizeDelta.x*.5f, Screen.height*.5f+image.rectTransform.sizeDelta.y*.5f );
    }

    //have to initialize Shader array with max amount we can send, or every other call it will truncate it down to the size of the first one
    private void InitializeShaderArray()
    {       
        for (int i = 0; i < MAX_MATERIAL_VECTOR_SIZE_ACTUAL; i++)
        {
            mouse_points_x.Add(-1.0f);
            mouse_points_y.Add(-1.0f);
            // list.Add(0.25f);
        }
        material.SetFloatArray("_Points_x", mouse_points_x);
        material.SetFloatArray("_Points_y", mouse_points_y);

        material.SetInt("_Points_Length", MAX_MATERIAL_VECTOR_SIZE_ACTUAL);

        //have to do it with the material too, to send over the max points
        // Graphics.Blit(rendertex, rendertex, material);

        StartCoroutine(EndOfFrameReached());        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            UpdateMouseOrTouchPoint();

            if(mouse_points_x.Count<MAX_MATERIAL_VECTOR_SIZE_ACTUAL&&max_points_set)
            {
                //don't add points to draw if it won't show up on the texture
                if(mouse_or_touch_point.x+brush_size_UV_half_x<0 || mouse_or_touch_point.y+brush_size_UV_half_y<0 || 
                   mouse_or_touch_point.x-brush_size_UV_half_x>1 || mouse_or_touch_point.y-brush_size_UV_half_y>1)
                {
                }
                else
                {
                    mouse_points_x.Add(mouse_or_touch_point.x);
                    mouse_points_y.Add(mouse_or_touch_point.y);
                }
            }
            else
                Debug.LogError("Too many points collected before being sent to shader!");
        }
    }
    public IEnumerator EndOfFrameReached()
    {
        while (true)
        {
            // We should only read the screen buffer after rendering is complete
            yield return new WaitForEndOfFrame();

            //to make sure the max size actually gets sent to the shader            
            if(mouse_points_x.Count==MAX_MATERIAL_VECTOR_SIZE_ACTUAL)
                max_points_set=true;

            if(max_points_set)
                SendPointsToShader();

            yield return null;
        }
    }

    private void SendPointsToShader()
    {
        if(mouse_points_x.Count>0)//it will hit this function before the initialization function has run
        {
            material.SetFloatArray("_Points_x", mouse_points_x);
            material.SetFloatArray("_Points_y", mouse_points_y);
            material.SetInt("_Points_Length", mouse_points_x.Count);

            mouse_points_x.Clear();
            mouse_points_y.Clear();

            // Graphics.Blit(rendertex, rendertex, material);
        }
    }

    private void UpdateMouseOrTouchPoint()
    {        
        if (Input.GetMouseButton(0) && Input.touchCount < 2)
            mouse_or_touch_point = Input.mousePosition;
        else if(Input.touchCount>0)
            mouse_or_touch_point = Input.GetTouch(0).position;

        ScreenCoordsToTexCoords();
    }
    private void ScreenCoordsToTexCoords()
    {
        //texture offset from bottom left
        mouse_or_touch_point.x = mouse_or_touch_point.x - rect_image_BL.x;
        mouse_or_touch_point.y = mouse_or_touch_point.y - rect_image_BL.y;
        
        //convert to texture UV coordinates
        mouse_or_touch_point.x/=rect_image_BL.width;
        mouse_or_touch_point.y/=rect_image_BL.height;
    }
}
