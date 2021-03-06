﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomFaceGenerator : MonoBehaviour {

    /* VARIABLES AND PROPERTIES */
    // Generator Options
    [Header("Generator Options")]
    public float shrinkChance = 0.25f;
    // Blank Prefabs
    [Header("Blank Prefabs")]
    public Face blankFace;
    public Body blankBody;
    // Body Feature Lists
    [Header("Body Generator Options")]
    public List<Color> shirtColors;
    // Face Features Lists
    [Header("Face Generator Options")]
    public List<GameObject> heads;
    public List<Color> skins;
    public List<Sprite> ears;
    public List<Sprite> eyes;
    public List<Sprite> noses;
    public List<Sprite> mouths;


    // TESTING VARS
    //float time_per_change = 0.5f;
    //float time_since_change = 0;
    //face cur_face;

    /* LIFECYCLE METHODS */
    private void Start () {
        //cur_face = GenerateRandomFace();
        //cur_face.DebugIndices();
    }

    private void Update () {
        //time_since_change += Time.deltaTime;
        //if(time_since_change >= time_per_change) {
        //    time_since_change = 0;
        //    Face next_face = GenerateRandomFaceFromParent(cur_face);
        //    DestroyImmediate(cur_face.gameObject);
        //    cur_face = next_face;
        //    cur_face.DebugIndices();
        //}
    }


    /* BODY-CENTRIC WRAPPERS FOR MAIN FUNCTIONS */
    public Body GenerateRandomBody () {
        Face face = GenerateRandomFace();
        Body body = Instantiate(blankBody);
        face.transform.SetParent(body.transform, false);
        body.shirtColor = GetRandomItemFromList<Color>(shirtColors);
        return body;
    }
    public Body GenerateRandomBodyFromParent (Body parent) {
        Face face = GenerateRandomFaceFromParent(parent.GetFace());
        Body body = Instantiate(blankBody);
        face.transform.SetParent(body.transform, false);
        body.shirtColor = parent.shirtColor;
        return body;
    }


    /* MAIN GENERATOR FUNCTIONS */
    // Generate a random face from scratch
    public Face GenerateRandomFace () {
        // Select features
        int head_index = GetRandomIndexFromList<GameObject>(heads);
        int skin_index = GetRandomIndexFromList<Color>(skins);
        int ear_index = GetRandomIndexFromList<Sprite>(ears);
        int eye_index = GetRandomIndexFromList<Sprite>(eyes);
        int nose_index = GetRandomIndexFromList<Sprite>(noses);
        int mouth_index = GetRandomIndexFromList<Sprite>(mouths);

        // Create new head from prefab
        Face face = Instantiate<Face>(blankFace);
        // Fill features on face
        face.SetHead(heads[head_index], head_index);
        face.SetSkin(skins[skin_index], skin_index);
        face.SetEars(ears[ear_index], ear_index);
        face.SetEyes(eyes[eye_index], eye_index);
        face.SetNose(noses[nose_index], nose_index);
        face.SetMouth(mouths[mouth_index], mouth_index);
        // Set random sizes on shrinkable features
        if (Random.value < 0.5)
            face.ToggleFeatureShrinkage(Face.Feature.Ears);
        if (Random.value < 0.5)
            face.ToggleFeatureShrinkage(Face.Feature.Eyes);
        if (Random.value < 0.5)
            face.ToggleFeatureShrinkage(Face.Feature.Nose);
        if (Random.value < 0.5)
            face.ToggleFeatureShrinkage(Face.Feature.Mouth);

        // Return finished head
        return face;
    }

    // Generate a random face from a parent
    public Face GenerateRandomFaceFromParent (Face parent) {
        int num_differences = 2; // NOTE: CAN MAKE THIS VALUE RANDOM LATER

        Face face = parent.Clone();
        List<Face.Feature> features = Face.BuildFeatureList();
        List<Face.Feature> shrinkableFeatures = Face.BuildShrinkableFeatureList();
        // Loop for num_differences times to change a different facial feature
        for(int i=0; i<num_differences; i++) {
            Face.Feature f = GetAndRemoveRandomItemFromList<Face.Feature>(features);
            // Perform shrinkage if randomness calls for it
            if (Face.IsShrinkableFeature(f) && shrinkableFeatures.Contains(f) && Random.value <= shrinkChance) {
                features.Add(f); // Re-add feature so it may be changed again
                shrinkableFeatures.Remove(f); // Remove feature so it can't be toggled again
                face.ToggleFeatureShrinkage(f);
            }
            else {
                switch (f) {
                    case Face.Feature.Head:
                        int head_index = GetRandomIndexFromList<GameObject>(heads, face.GetIndexOf(Face.Feature.Head));
                        face.SetHead(heads[head_index], head_index);
                        break;

                    case Face.Feature.Skin:
                        int skin_index = GetRandomIndexFromList<Color>(skins, face.GetIndexOf(Face.Feature.Skin));
                        face.SetSkin(skins[skin_index], skin_index);
                        break;

                    case Face.Feature.Ears:
                        int ear_index = GetRandomIndexFromList<Sprite>(ears, face.GetIndexOf(Face.Feature.Ears));
                        face.SetEars(ears[ear_index], ear_index);
                        break;

                    case Face.Feature.Eyes:
                        int eye_index = GetRandomIndexFromList<Sprite>(eyes, face.GetIndexOf(Face.Feature.Eyes));
                        face.SetEyes(eyes[eye_index], eye_index);
                        break;

                    case Face.Feature.Nose:
                        int nose_index = GetRandomIndexFromList<Sprite>(noses, face.GetIndexOf(Face.Feature.Nose));
                        face.SetNose(noses[nose_index], nose_index);
                        break;

                    case Face.Feature.Mouth:
                        int mouth_index = GetRandomIndexFromList<Sprite>(mouths, face.GetIndexOf(Face.Feature.Mouth));
                        face.SetMouth(mouths[mouth_index], mouth_index);
                        break;

                    default:
                        Debug.LogError("Missing case for certain Feature in GenerateRandomFaceFromParent!");
                        break;
                }
            }
        }
        return face;
    }


    /* HELPER FUNCTIONS */
    int GetRandomIndexFromList<T> (List<T> list) {
        if (list.Count == 0)
        {
            Debug.LogWarning("GetRandomIndexOfList: list is empty.");
            return -1;
        }

        int index = (int)(Random.value * list.Count);
        if (index == list.Count)
            index--;
        return index;
    }
    int GetRandomIndexFromList<T> (List<T> list, int excluding_index) {
        if (list.Count <= 1) {
            Debug.LogWarning("GetRandomIndexOfList: list is too small.");
            return -1;
        }

        int index = (int)(Random.value * (list.Count-1));
        if (index == list.Count-1)
            index--;
        if (index >= excluding_index)
            index++;
        return index;
    }
    T GetRandomItemFromList<T> (List<T> list) {
        int index = GetRandomIndexFromList<T>(list);
        if (index < 0)
            return default(T);
        return list[index];
    }
    T GetAndRemoveRandomItemFromList<T> (List<T> list) {
        int index = GetRandomIndexFromList<T>(list);
        if (index < 0)
            return default(T);
        T item = list[index];
        list.RemoveAt(index);
        return item;
    }
}
