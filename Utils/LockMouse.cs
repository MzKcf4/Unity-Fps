using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockMouse : MonoBehaviour
{
	public void Toggle()
	{
		Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? 
												 CursorLockMode.None : CursorLockMode.Locked;
	}
}
