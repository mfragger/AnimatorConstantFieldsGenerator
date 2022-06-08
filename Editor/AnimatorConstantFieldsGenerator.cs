using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Windows;

namespace BarelyAStudio.AnimatorConstantFieldsGenerator
{
    public class AnimatorConstantFieldsGenerator : MonoBehaviour
    {
        public class AnimatorPostProcessCodeGenerator : AssetPostprocessor
        {
            private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                GenerateCode();
            }

            private static void GenerateCode()
            {
                var nameSpace = AnimatorConstantFieldsGeneratorSettings.GetOrCreateSettings().@namespace;
#if UNITY_2021_2_OR_NEWER
                StringBuilder stringBuilder = new();
#elif UNITY_2020_3_OR_NEWER
                StringBuilder stringBuilder = new StringBuilder();
#endif

                var results = AssetDatabase.FindAssets("t:AnimatorController");

                stringBuilder.AppendLine("using UnityEngine;");
                stringBuilder.AppendLine();

                stringBuilder.AppendLine($"namespace {nameSpace}");
                stringBuilder.AppendLine("{");
                {

                    for (int i = 0; i < results.Length; i++)
                    {

                        var animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(results[i]));

                        var clips = animatorController.animationClips;
                        var layers = animatorController.layers;
                        var paramters = animatorController.parameters;

                        var areThereClips = clips.Length > 0;
                        var areThereLayers = layers.Length > 0;

                        var areThereParameters = paramters.Length > 0;

                        if (areThereClips || areThereLayers || areThereParameters)
                        {
                            if (i > 0)
                            {
                                stringBuilder.AppendLine();
                            }
                            var animatorControllerName = animatorController.name;
                            stringBuilder.AppendLine($"\tpublic class {animatorControllerName.Replace(" ", "")}");
                            stringBuilder.AppendLine("\t{");
                            {
                                if (areThereClips)
                                {
                                    stringBuilder.AppendLine($"\t\t/// <summary>");
                                    stringBuilder.AppendLine($"\t\t/// The animation clip names that are attached to all animator states. <br /><br />");
                                    stringBuilder.AppendLine($"\t\t/// Majority of the API in the <see cref=\"Animator\"/> uses the state rather than the animation clip. <br />");
                                    stringBuilder.AppendLine($"\t\t/// When you create a state for the first time, it's often the case that the the State.name == Animation.name. <br />");
                                    stringBuilder.AppendLine($"\t\t/// So <see cref=\"Animator.Play(string)\"/> works when you pass in the name of the animation clip. <br /><br />");
                                    stringBuilder.AppendLine($"\t\t/// If your animation isn't playing from <see cref=\"Animator.Play(string)\"/>, <br />");
                                    stringBuilder.AppendLine($"\t\t/// then you have set the name of the State differently than your Animation. <br />");
                                    stringBuilder.AppendLine($"\t\t///Pass in any of the fields in the States struct within the <see cref=\"Layers\"/> struct instead. <br />");
                                    stringBuilder.AppendLine($"\t\t/// </summary>");
                                    stringBuilder.AppendLine($"\t\tpublic readonly struct Animations");
                                    stringBuilder.AppendLine("\t\t{");
                                    {
#if UNITY_2021_2_OR_NEWER
                                        Dictionary<string, int> existingName = new();
#elif UNITY_2020_3_OR_NEWER
                                    Dictionary<string, int> existingName = new Dictionary<string, int>();
#endif
                                        for (int j = 0; j < clips.Length; j++)
                                        {
                                            var animationName = clips[j].name;

                                            if (existingName.ContainsKey(animationName))
                                            {
                                                existingName[animationName]++;
                                            }
                                            else
                                            {
                                                existingName.Add(animationName, 0);
                                            }

                                            var upperedName = animationName.ToUpper();

                                            string counter = existingName[animationName] > 0 ? $"_{existingName[animationName]}" : "";

                                            stringBuilder.AppendLine($"\t\t\tpublic const string {Regex.Replace(upperedName, @"[^\w]", "_")}{counter} = \"{animationName}\";");
                                        }
                                    }
                                    stringBuilder.AppendLine("\t\t}");
                                }

                                if (areThereLayers)
                                {
                                    stringBuilder.AppendLine($"\t\tpublic readonly struct Layers");
                                    stringBuilder.AppendLine("\t\t{");
                                    {
                                        StringBuilder stateMachineStringBuilder = null;
                                        for (int j = 0; j < layers.Length; j++)
                                        {
                                            var layerName = layers[j].name;
                                            var regexedlayerName = Regex.Replace(layerName, @"[^\w]", "_");

                                            stringBuilder.AppendLine($"\t\t\t/// <summary>");
                                            stringBuilder.AppendLine($"\t\t\t/// One of the <see cref=\"Layers\"/> in the <see cref=\"{animatorControllerName}\"/> Animator.");
                                            stringBuilder.AppendLine($"\t\t\t/// </summary>");
                                            stringBuilder.AppendLine($"\t\t\tpublic readonly struct {regexedlayerName}");
                                            stringBuilder.AppendLine("\t\t\t{");
                                            {
                                                stringBuilder.AppendLine($"\t\t\t\tpublic const string Name = \"{layerName}\";");
                                                stringBuilder.AppendLine($"\t\t\t\tpublic const int Index = {j};");
                                                var stateMachine = layers[j].stateMachine;
                                                var states = stateMachine.states;

                                                if (stateMachine != null && states.Length > 0)
                                                {
                                                    stateMachineStringBuilder = new StringBuilder();
                                                    stateMachineStringBuilder.AppendLine($"\t\t\t\t/// <summary>");
                                                    stateMachineStringBuilder.AppendLine($"\t\t\t\t/// The states in the <see cref=\"Layers.{layerName}\"/>.");
                                                    stateMachineStringBuilder.AppendLine($"\t\t\t\t/// </summary>");
                                                    stateMachineStringBuilder.AppendLine($"\t\t\t\tpublic readonly struct States");
                                                    stateMachineStringBuilder.AppendLine("\t\t\t\t{");
                                                    {
                                                        var upperedName = stateMachine.defaultState.name.ToUpper();

                                                        stateMachineStringBuilder.AppendLine($"\t\t\t\t\tpublic const int DEFAULT_STATE = {Regex.Replace(upperedName, @"[^\w]", "_")};");
                                                        for (int s = 0; s < states.Length; s++)
                                                        {
                                                            var state = states[s].state;
                                                            var stateName = state.name;

                                                            upperedName = stateName.ToUpper();

                                                            stateMachineStringBuilder.AppendLine($"\t\t\t\t\tpublic const int {Regex.Replace(upperedName, @"[^\w]", "_")} = {Animator.StringToHash(stateName)};");
                                                        }
                                                    }
                                                    stateMachineStringBuilder.AppendLine("\t\t\t\t}");
                                                    stringBuilder.Append(stateMachineStringBuilder.ToString());
                                                }
                                            }
                                            stringBuilder.AppendLine("\t\t\t}");
                                        }
                                    }
                                    stringBuilder.AppendLine("\t\t}");
                                }

                                if (areThereParameters)
                                {

                                    stringBuilder.AppendLine($"\t\tpublic readonly struct Parameters");
                                    stringBuilder.AppendLine("\t\t{");
                                    {

                                        for (int j = 0; j < paramters.Length; j++)
                                        {
                                            var parameterName = paramters[j].name;
                                            stringBuilder.AppendLine($"\t\t\tpublic const int {parameterName.ToUpper().Replace(" ", "_")} = {Animator.StringToHash(parameterName)};");
                                        }

                                    }
                                    stringBuilder.AppendLine("\t\t}");
                                }
                            }
                            stringBuilder.AppendLine("\t}");
                        }
                    }
                }
                stringBuilder.AppendLine("}");

                var bytes = Encoding.ASCII.GetBytes(stringBuilder.ToString());

                var filePath = AnimatorConstantFieldsGeneratorSettings.GetOrCreateSettings().fileName;

                if (File.Exists(filePath))
                {
                    var readBytes = File.ReadAllBytes(filePath);

                    if (!readBytes.SequenceEqual(bytes))
                    {
                        WriteToFile(bytes);
                    }
                }
                else
                {
                    WriteToFile(bytes);
                }
            }

            private static void WriteToFile(byte[] bytes)
            {
                var filePath = AnimatorConstantFieldsGeneratorSettings.GetOrCreateSettings().fileName;

                Debug.Log($"Generating \"{filePath}\"");
                File.WriteAllBytes(filePath, bytes);
                CompilationPipeline.RequestScriptCompilation();
            }
        }
    }
}
