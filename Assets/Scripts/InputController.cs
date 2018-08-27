using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    private Vector2 touchingPosition;
    private bool dragging, touched;
    
	void Start ()
    {
        // Reset flags
        dragging = false;
        touched = false;
    }
	
	void Update ()
    {
        // Are we touching or dragging ?
        if (touched || dragging)
        {
            // Are we still holding down ?
            dragging = Input.GetMouseButton(0);
            if (dragging)
                // Set the holding down position
                touchingPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Always reset the touched flag
            touched = false;
        }
        else
        {
            // Did we touch the screen for the first time
            touched = Input.GetMouseButton(0);
            if (touched)
                // Set the holding down position
                touchingPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    /*
     * Will detect whether or not the screen has been touched for the first time (not dragging) 
     */
    public bool IsTouched()
    {
        return touched;
    }

    /*
     * Will return the current touching position
     */
    public Vector2 GetTouchingPosition()
    {
        return touchingPosition;
    }

    /*
     * Will detect whether or not we're dragging on the screen
     */
    public bool IsDragging()
    {
        return dragging;
    }
}
