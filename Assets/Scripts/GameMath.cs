using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameMath
{
	public static bool IsInRange(Vector3 position1, Vector3 position2, Vector3 size, float range)
	{
		return Mathf.Abs((float)GetDistanceBetweenVectors(position1, position2)) < size.x * range;
	}

    /*
     * This will calculate the distance between two given vectors
     */
    public static double GetDistanceBetweenVectors(Vector3 firstVector, Vector3 secondVector)
	{
		// Get the distance between points
		Vector2 distance = new Vector2(secondVector.x - firstVector.x, secondVector.y - firstVector.y);
		return Mathf.Sqrt(distance.x * distance.x + distance.y * distance.y);
	}

    /*
     * This will calculate the angle between two given vectors
     * Returns the angle in degrees
     */
    public static float GetAngleBetweenVectors(Vector2 origin, Vector2 target)
    {
        float angle = Mathf.Atan2(target.y - origin.y, target.x - origin.x) * Mathf.Rad2Deg;
        return angle < 0 ? angle + 360 : angle;
    }
}
