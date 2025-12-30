using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ActionCooldown
{
	public float interval = 5f;
	public float nextUse;
	
    public void ReduceCooldown()
    {
        ReduceCooldown(Time.deltaTime);
    }
    
	public void ReduceCooldown(float amt)
	{
		if(nextUse <= 0f)	return;
		nextUse -= amt;
	}

    public void InstantCooldown()
    {
        nextUse = 0f;
    }
	
	public bool IsOnCooldown()
	{
        return IsOnCooldown(false);
	}
    
    public bool IsOnCooldown(bool reduceCooldown)
    {
        if(reduceCooldown)
            ReduceCooldown(Time.deltaTime);
            
        return nextUse > 0f;
    }
    
    public bool CanExecuteAfterDeltaTime()
    {
        return CanExecuteAfterDeltaTime(false);
    }
    
    
    public bool CanExecuteAfterDeltaTime(bool resetCooldownIfAvailable)
    {
        ReduceCooldown(Time.deltaTime);
        
        if(!IsOnCooldown())
        {
            if(resetCooldownIfAvailable)
                StartCooldown();
            
            return true;
        }
        
        return false;
    }
    
    public void StartCooldown(float newInterval)
    {
        nextUse = newInterval;
    }
	
	public void StartCooldown()
	{
		nextUse = interval;
	}
}
