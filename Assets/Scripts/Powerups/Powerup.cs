using System;
using UnityEngine;

public enum PowerupEffects
{
  grow,
  shrink,
  slow,
  Count
}

[Serializable]
public struct Powerup
{
  public GameObject obj;
  public PowerupEffects effect;
}