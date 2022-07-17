using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
[AddComponentMenu("S4 scn/Collision Data")]
public class CollisionData : MonoBehaviour
{
    public GroundFlag ground = GroundFlag.NONE;
    public WeaponFlag weapon = WeaponFlag.NONE;
}

public enum GroundFlag
{
    NONE,
    blast,
    land_ground,
    land_stone,
    land_steel,
    land_wood,
    land_water,
    land_glass,
    land_ground_wire,
    land_stone_wire,
    land_steel_wire,
    land_wood_wire,
    land_water_wire,
    land_glass_wire,
    land_ground_hash,
    land_stone_hash,
    land_steel_hash,
    land_wood_hash,
    land_water_hash,
    land_glass_hash,
    land_ground_wirehash,
    land_stone_wirehash,
    land_steel_wirehash,
    land_wood_wirehash,
    land_water_wirehash,
    land_glass_wirehash,
    block_alpha,
    block_beta,
}
public enum WeaponFlag
{
    NONE, weapon, weapon_wire
}