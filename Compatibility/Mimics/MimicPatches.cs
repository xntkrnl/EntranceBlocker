using DunGen;
using HarmonyLib;
using Mimics;
using Mimics.API;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EntranceBlocker.Compatibility.Mimics
{
    internal class MimicPatches
    {
        //im not sure if im supposed to patch this or i should subscribe my method to some event that i didnt notice
        //[HarmonyPostfix, HarmonyPatch(typeof(MimicEventHandler), nameof(MimicEventHandler.OnMimicCreated))]
        //static void OnMimicCreatedPatch(MimicDoor mimicDoor, Doorway doorway)
        //{

        //}
    }
}
