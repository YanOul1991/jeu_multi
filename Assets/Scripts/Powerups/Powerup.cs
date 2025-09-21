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
  public ulong obj;
  public PowerupEffects effect;
}