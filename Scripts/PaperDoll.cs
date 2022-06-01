using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperDoll : MonoBehaviour
{
    public enum Type
    {
        hair = 100,
        face,
        body,
        leg,
        hand,
        foot,
        acc,
        pet,
        NONE
    }

    public bool isGirl;

    public List<Container> attachedHeads = new List<Container>();
    public List<Container> attachedFaces = new List<Container>();
    public List<Container> attachedShirts = new List<Container>();
    public List<Container> attachedPants = new List<Container>();
    public List<Container> attachedGloves = new List<Container>();
    public List<Container> attachedShoes = new List<Container>();
    public List<Container> attachedAccesories = new List<Container>();
    public List<Container> attachedPets = new List<Container>();

    public List<Container> GetAttachedParts(Type part)
	{
        if (part == Type.hair)
        {
            return attachedHeads;
        }
        if (part == Type.face)
        {
            return attachedFaces;
        }
        if (part == Type.body)
        {
            return attachedShirts;
        }
        if (part == Type.leg)
        {
            return attachedPants;
        }
        if (part == Type.hand)
        {
            return attachedGloves;
        }
        if (part == Type.foot)
        {
            return attachedShoes;
        }
        if (part == Type.acc)
        {
            return attachedAccesories;
        }
        if (part == Type.pet)
        {
            return attachedPets;
        }
        return null;
    }

    public struct Container
	{
        public List<ScnData> parts;
        public string iconName;

        public Container(string iconName)
        {
            this.iconName = iconName;
            parts = new List<ScnData>();
        }
        public Container(string iconName, List<ScnData> parts)
        {
            this.iconName = iconName;
            this.parts = parts;
        }
        public Container(string iconName, ScnData part)
        {
            this.iconName = iconName;
            this.parts = new List<ScnData>();
            parts.Add(part);
        }
    }
}
