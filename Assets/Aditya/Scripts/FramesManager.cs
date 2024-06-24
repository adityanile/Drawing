using IDZBase.Core.GameTemplates.Coloring;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using XDPaint;
using XDPaint.Controllers;
using XDPaint.Core;
using XDPaint.Tools.Image;

public class FramesManager : MonoBehaviour
{
    public List<GameObject> paintables;

    public UnityEvent<PaintManager> OnPointerDown = new();
    public UnityEvent<PaintManager> OnPointerUp = new();
    public BrushData CurrentBrushData;

    private void Start()
    {
        StartCoroutine(Initailise());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(0);
        }
    }
    IEnumerator Initailise()
    {
        foreach (var part in paintables)
        {
            var paintManager = part.GetComponent<PaintManager>();
            part.AddComponent<PolygonCollider2D>();

            var eventTrigger = part.AddComponent<EventTrigger>();


            if (part.CompareTag("Glow"))
            {
                Material mat = part.GetComponent<Material>();   
            }


            var pointerDownEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };

            pointerDownEntry.callback.AddListener(eventData =>
            {
                OnPointerDown.Invoke(paintManager);

                paintManager.PaintObject.ProcessInput = true;
                InitBrush(CurrentBrushData, paintManager);
            });

            eventTrigger.triggers.Add(pointerDownEntry);

            var pointerUpEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerUp
            };

            pointerUpEntry.callback.AddListener(_ =>
            {
                OnPointerUp.Invoke(paintManager);

                paintManager.PaintObject.ProcessInput = false;
                paintManager.PaintObject.FinishPainting();
            });

            eventTrigger.triggers.Add(pointerUpEntry);


            yield return new WaitUntil(() => paintManager.Initialized);

            paintManager.PaintObject.ProcessInput = false;
        }
    }

    public void InitBrush(BrushData brushData, PaintManager paintManager)
    {
        if (paintManager is null) return;
        if (paintManager.ToolsManager.CurrentTool.Type != PaintTool.Brush) return;
        if (brushData.UsePattern)
        {
            PaintController.Instance.UseSharedSettings = false;
            paintManager.SetPaintMode(PaintMode.Additive);
            paintManager.Brush.SetColor(brushData.BrushColor);
            paintManager.Brush.Size = brushData.BrushSize;
            var settings = ((BrushTool)paintManager.ToolsManager.CurrentTool).Settings;
            settings.UsePattern = true;
            settings.PatternTexture = brushData.PatternTexture;
            settings.PatternScale = brushData.PatternScale;
        }
        else
        {
            PaintController.Instance.UseSharedSettings = true;

            PaintController.Instance.Brush.SetColor(brushData.BrushColor);
            PaintController.Instance.Brush.Size = brushData.BrushSize;

            paintManager.SetPaintMode(PaintMode.Default);
            var settings = ((BrushTool)paintManager.ToolsManager.CurrentTool).Settings;
            settings.UsePattern = false;
        }
    }

}
