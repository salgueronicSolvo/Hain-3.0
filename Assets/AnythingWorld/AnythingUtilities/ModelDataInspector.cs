using AnythingWorld.Utilities.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnythingWorld.Utilities
{
    [Serializable]
    public class ModelDataInspector : MonoBehaviour
    {
        [SerializeField]
        public string guid = "";
        [SerializeField]
        public string behaviour = "";
        [SerializeField]
        public string[] tags = { ""};
        [SerializeField]
        public string[] habitats = { ""};
        [SerializeField]
        public string author = "";
        [SerializeField]
        public string entity = "";
        [SerializeField]
        public Dictionary<string, float> scales;
        [SerializeField]
        public Dictionary<string, float> movement;

        public void Populate(ModelData data)
        {
            guid = data.json.name;
            behaviour = data.json.behaviour;
            tags = data.json.tags;
            scales = data.json.scale;
            movement = data.json.movement;
            entity = data.json.entity;
            author = data.json.author;
            habitats = data.json.habitats;
        }
    }

}