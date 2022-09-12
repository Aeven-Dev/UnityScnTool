using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace AevenScnTool
{
    public class AnimationWindow : EditorWindow
    {

        public int frame = 0;
        public string selectedAnimName = "";

        AnimationWrapper animation;
        public List<string> animationNames = new List<string>();

        Button attachInd;
        Button attachArm;
        Button attachScene;
        Button detach;

        Button import;
        Button export;

        Button switchToTransformEditor;
        Button switchToUVEditor;

        Button addAnim;
        Button removeAnim;

        TransformEditor transformEditor;
        UVAnimEditor uvAnimEditor;

        ListView animationList;
        TextField animName;
        VisualElement channelList;

        bool attached = false;

        bool editingTransform = true;

        Dictionary<S4Animations, S4Animation> bones;
        Dictionary<S4Animations, S4Animation> copies;

        private void OnSelectionChange()
        {
            CheckSelectedObject();
        }

        private void OnHierarchyChange()
        {
            if (animation?.GetRoot() == null)
            {
                Detach();
            }
        }

        [MenuItem("Window/S4 Scn/Animation Window! :D")]
        public static void Init()
        {
            AnimationWindow wnd = GetWindow<AnimationWindow>();
            wnd.titleContent = new GUIContent("AnimationWindow");
        }

        public void CreateGUI()
        {
            //Init------------------------------
            VisualElement root = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AevenScnTool.ScnToolData.RootPath + "Editor/Window/AnimationEditor/AnimationWindow.uxml");
            visualTree.CloneTree(root);

            //Getting references----------------
            GetGUIReferences();

            //Setting up things-----------------

            CheckSelectedObject();// Check the selected object to enable some stuff
            switchToTransformEditor.SetEnabled(false);
            SetCallbacks();

            export.SetEnabled(false);
            import.SetEnabled(false);

            var anim = new SerializedObject(this);

            animationNames.Clear();
            animName.SetEnabled(false);//Start disabled because no anim would be selected
            animName.RegisterValueChangedCallback(ChangeAnimName);//Changing this value will change the anim name
            animationList.Bind(anim);
            animationList.makeItem = () => { return new Label(); };
            animationList.onSelectedIndicesChange += AnimationChanged;
        }

        void GetGUIReferences()
        {
            attachInd = rootVisualElement.Q("AttachInd") as Button;
            attachArm = rootVisualElement.Q("AttachArm") as Button;
            attachScene = rootVisualElement.Q("AttachScene") as Button;
            detach = rootVisualElement.Q("Detach") as Button;

            import = rootVisualElement.Q("ImportBtn") as Button;
            export = rootVisualElement.Q("ExportBtn") as Button;

            switchToTransformEditor = rootVisualElement.Q<Button>("SwitchTransformEditor");
            switchToUVEditor = rootVisualElement.Q<Button>("SwitchUVAnimEditor");

            animationList = rootVisualElement.Q("AnimList") as ListView;
            animName = rootVisualElement.Q("AnimName") as TextField;

            ScrollView partContainerScrollview = rootVisualElement.Q("Scrollview") as ScrollView;
            channelList = partContainerScrollview.contentContainer;
            transformEditor = rootVisualElement.Q<TransformEditor>("TransformEditor");
            uvAnimEditor = rootVisualElement.Q<UVAnimEditor>("UVAnimEditor");

            addAnim = rootVisualElement.Q<Button>("AddAnimation");
            removeAnim = rootVisualElement.Q<Button>("RemoveAnimation");
        }

        void SetCallbacks()
        {
            attachInd.clicked += AttachToIndividualObject;
            attachArm.clicked += AttachToArmature;
            attachScene.clicked += AttachToScene;
            detach.clicked += Detach;

            export.clicked += Export;
            import.clicked += Import;

            switchToTransformEditor.clicked += SwitchToTransformEditor;
            switchToUVEditor.clicked += SwitchToUVAnimEditor;

            addAnim.clicked += AddAnimation;
            removeAnim.clicked += RemoveAnimation;

            transformEditor.RegisterSetTotalFramesCallback(e => SetTotalFrames(e.newValue));
            uvAnimEditor.RegisterSetTotalFramesCallback(e => SetTotalFrames(e.newValue));
        }

        void CheckSelectedObject()
        {
            if (Selection.activeGameObject)
            {
                if (Selection.activeGameObject.GetComponent<S4Animations>())
                {

                    attachInd.SetEnabled(true);
                    attachArm.SetEnabled(true);
                    attachScene.SetEnabled(false);

                    transformEditor.ToggleKeying(attached);
                }
                else if (Selection.activeGameObject.GetComponent<ScnData>())
                {
                    attachInd.SetEnabled(false);
                    attachArm.SetEnabled(false);
                    attachScene.SetEnabled(true);
                    transformEditor.ToggleKeying(false);
                }
                else
                {
                    attachInd.SetEnabled(false);
                    attachArm.SetEnabled(false);
                    attachScene.SetEnabled(false);
                    transformEditor.ToggleKeying(false);
                }
            }
            else
            {
                attachInd.SetEnabled(false);
                attachArm.SetEnabled(false);
                attachScene.SetEnabled(false);
                transformEditor.ToggleKeying(false);
            }
        }

        void AttachToIndividualObject()
        {
            var anims = Selection.activeGameObject.GetComponent<S4Animations>();
            bool isMesh = Selection.activeGameObject.GetComponent<Bone>() == null;
            if (anims)
            {
                ClearAnimations();
                animation = new SingleAnimationWrapper(anims, isMesh);
                animationNames = animation.GetAnimationNames();
                selectedAnimName = string.Empty;

                SwapAttachButtons(false);
            }
            else
            {
                Debug.LogWarning("Oh no! I failed to attach the object! t.t help me debug it!", Selection.activeGameObject);
            }
        }

        void AttachToArmature()
        {
            var root = Selection.activeGameObject.GetComponent<S4Animations>();
            if (root)
            {
                ClearAnimations();
                animation = new ArmatureAnimationWrapper(root);
                animationNames = animation.GetAnimationNames();
                selectedAnimName = string.Empty;

                SwapAttachButtons(false);
            }
            else
            {
                Debug.LogWarning("Oh no! I failed to attach the object! t.t help me debug it!", Selection.activeGameObject);
                return;
            }
        }

        void AttachToScene()
        {
            var root = Selection.activeGameObject.GetComponent<ScnData>();
            if (root)
            {
                ClearAnimations();
                animation = new SceneAnimationWrapper(root);
                animationNames = animation.GetAnimationNames();
                selectedAnimName = string.Empty;

                SwapAttachButtons(false);
            }
            else
            {
                Debug.LogWarning("Oh no! I failed to attach the object! t.t help me debug it!", Selection.activeGameObject);
                return;
            }
        }

        void Detach()
        {
            SwapAttachButtons(true);
            ClearAnimations();

            import.SetEnabled(false);

            transformEditor.AnimationCleared();
        }

        void SwapAttachButtons(bool state)
        {
            if (state)
            {
                attachInd.style.display = DisplayStyle.Flex;
                attachArm.style.display = DisplayStyle.Flex;
                attachScene.style.display = DisplayStyle.Flex;
                detach.style.display = DisplayStyle.None;
                attached = false;
            }
            else
            {
                attachInd.style.display = DisplayStyle.None;
                attachArm.style.display = DisplayStyle.None;
                attachScene.style.display = DisplayStyle.None;
                detach.style.display = DisplayStyle.Flex;
                attached = true;
            }
        }

        void AnimationChanged(IEnumerable<int> selectedIndeces)
        {
            int newIndex = new List<int>(selectedIndeces)[0];

            if (selectedAnimName == animationNames[newIndex])
            {
                return;
            }
            selectedAnimName = animationNames[newIndex];
            animName.SetValueWithoutNotify(selectedAnimName);
            animName.SetEnabled(true);

            frame = 0;

            export.SetEnabled(true);

            if (editingTransform)
            {
                SetTransformBones();
                transformEditor.AnimationChanged(bones, copies);
            }
            else
            {
                SetUVBones();
                uvAnimEditor.AnimationChanged(bones, copies);
            }

        }

        void ClearAnimations()
        {
            animation = null;
            animationNames.Clear();
            selectedAnimName = string.Empty;
            channelList.Clear();
            animName.SetEnabled(false);
            export.SetEnabled(false);

            animName.SetValueWithoutNotify(string.Empty);
        }

        void SetTransformBones()
        {
            channelList.Clear();
            transformEditor.ClearChannels();
            bones = animation.GetParts(selectedAnimName);
            copies = animation.GetCopies(selectedAnimName);

            bool alternateBackground = true;
            Color bg1 = new Color(0.3f, 0.3f, 0.3f);
            Color bg2 = new Color(0.275f, 0.275f, 0.275f);

            foreach (var bone in bones)
            {
                alternateBackground = !alternateBackground;
                var parent = new ChannelItem(bone.Key.name, alternateBackground ? bg1 : bg2, transformEditor, uvAnimEditor, bone.Value);
                /* var gbl = new Toggle();
                 var foldout = new Foldout();
                 var pos = new Toggle();
                 var rot = new Toggle();
                 var sca = new Toggle();
                 var alp = new Toggle();

                 gbl.SetValueWithoutNotify(true);
                 foldout.text = bone.name;
                 foldout.value = false;

                 pos.text = "Position"; rot.text = "Rotation"; sca.text = "Scale"; alp.text = "Alpha";
                 pos.style.paddingLeft = rot.style.paddingLeft = sca.style.paddingLeft = alp.style.paddingLeft = 30;

                 pos.SetValueWithoutNotify(false);
                 rot.SetValueWithoutNotify(false);
                 sca.SetValueWithoutNotify(false);
                 alp.SetValueWithoutNotify(false);

                 pos.style.backgroundColor = rot.style.backgroundColor = sca.style.backgroundColor = alp.style.backgroundColor = bg;

                 foldout.Add(pos);
                 foldout.Add(rot);
                 foldout.Add(sca);
                 foldout.Add(alp);

                 gbl[0].style.flexGrow = 0f;
                 gbl[0][0].style.alignSelf = Align.FlexStart;
                 parent.Add(gbl);
                 parent.Add(foldout);
                 parent.style.flexDirection = FlexDirection.Row;

                 gbl.RegisterValueChangedCallback((evnt) => {BoneChannelGlobalCallback(evnt, bones[bone].TransformKeyData);});

                 TransformKeyData tkd = bones[bone].TransformKeyData;
                 pos.RegisterValueChangedCallback((evnt) => {
                     transformEditor.SetChannel(tkd, evnt.newValue, TransformChannel.Channel.Position);
                     evnt.StopPropagation();
                 });
                 rot.RegisterValueChangedCallback((evnt) => {
                     transformEditor.SetChannel(tkd, evnt.newValue, TransformChannel.Channel.Rotation);
                     evnt.StopPropagation();
                 });
                 sca.RegisterValueChangedCallback((evnt) => {
                     transformEditor.SetChannel(tkd, evnt.newValue, TransformChannel.Channel.Scale);
                     evnt.StopPropagation();
                 });
                 alp.RegisterValueChangedCallback((evnt) => {
                     transformEditor.SetChannel(tkd, evnt.newValue, TransformChannel.Channel.Alpha);
                     evnt.StopPropagation();
                 });*/

                channelList.Add(parent);
                transformEditor.AddChannel(bone.Value.TransformKeyData);
            }
        }

        void SetUVBones()
        {
            channelList.Clear();
            transformEditor.ClearChannels();
            bones = animation.GetParts(selectedAnimName);
            copies = animation.GetCopies(selectedAnimName);

            bool alternateBackground = true;
            Color bg1 = new Color(0.3f, 0.3f, 0.3f);
            Color bg2 = new Color(0.275f, 0.275f, 0.275f);

            foreach (var bone in bones)
            {
                alternateBackground = !alternateBackground;
                var parent = new ChannelItem(bone.Key.name, alternateBackground ? bg1 : bg2, transformEditor, uvAnimEditor, bone.Value);

                channelList.Add(parent);
                transformEditor.AddChannel(bone.Value.TransformKeyData);
            }
        }
        /*
        void BoneChannelGlobalCallback(ChangeEvent<bool> e, TransformKeyData tkd)
        {
            Foldout foldout = ((VisualElement)(e.target)).parent[1] as Foldout;
            Toggle pos = foldout[0] as Toggle;
            Toggle rot = foldout[1] as Toggle;
            Toggle sca = foldout[2] as Toggle;
            Toggle alp = foldout[3] as Toggle;
            if (e.newValue)
            {
                pos.SetEnabled(true);
                rot.SetEnabled(true);
                sca.SetEnabled(true);
                alp.SetEnabled(true);
                transformEditor.SetAllChannels(tkd, pos.value);
            }
            else
            {
                pos.SetEnabled(false);
                rot.SetEnabled(false);
                sca.SetEnabled(false);
                alp.SetEnabled(false);
                transformEditor.SetAllChannels(tkd, false);
            }
            e.StopPropagation();
        }*/

        void Export()
        {
            AnimIO.Export(selectedAnimName, animation);
        }
        void Import()
        {
            AnimIO.Import(animation);
            animationNames = animation.GetAnimationNames();
            selectedAnimName = string.Empty;
        }

        void ChangeAnimName(ChangeEvent<string> evnt)
        {
            if (evnt.newValue == string.Empty)
            {
                animName.SetValueWithoutNotify(selectedAnimName);
                return;
            }
            animation.ChangeAnimationName(selectedAnimName, evnt.newValue);
            selectedAnimName = evnt.newValue;
        }


        void SwitchToTransformEditor()
        {
            editingTransform = true;
            switchToTransformEditor.SetEnabled(false);
            switchToUVEditor.SetEnabled(true);

            transformEditor.SetEnabled(true);
            transformEditor.style.display = DisplayStyle.Flex;

            uvAnimEditor.SetEnabled(false);
            uvAnimEditor.style.display = DisplayStyle.None;

            if (selectedAnimName != string.Empty && animation != null)
            {
                SetTransformBones();
                transformEditor.AnimationChanged(bones, copies);
            }
        }

        void SwitchToUVAnimEditor()
        {
            editingTransform = false;
            switchToTransformEditor.SetEnabled(true);
            switchToUVEditor.SetEnabled(false);

            transformEditor.SetEnabled(false);
            transformEditor.style.display = DisplayStyle.None;

            uvAnimEditor.SetEnabled(true);
            uvAnimEditor.style.display = DisplayStyle.Flex;

            if (selectedAnimName != string.Empty && animation != null)
            {
                SetUVBones();
                uvAnimEditor.AnimationChanged(bones, copies);
            }
        }

        void AddAnimation()
        {
            animation.AddAnimation(new string[] { "SWAY", "SPIN", "WHIRL", "TWIRL", "PIROUETTE", "PRANCE", "JIG", "HOP", "BOB", "BOUNCE" }[Random.Range(0, 10)] + "_" + animationNames.Count);
            animationNames = animation.GetAnimationNames();
            animationList.SetSelection(animationNames.Count - 1);
            animName.Focus();
        }

        void RemoveAnimation()
        {
            if (selectedAnimName != string.Empty)
            {
                animation.RemoveAnimation(selectedAnimName);
                animationNames = animation.GetAnimationNames();
            }
        }

        void SetTotalFrames(int frames)
        {
            animation.SetTotalFrames(selectedAnimName, frames);
        }
        void OnDestroy()
        {
            transformEditor.OnDestroy();
            uvAnimEditor.OnDestroy();
        }
    }

    class ChannelItem : VisualElement
    {
        static Color bg = new Color(0.2196079f, 0.2196079f, 0.2196079f);
        TransformEditor _transformEditor;
        UVAnimEditor _uvanimEditor;
        bool displayingTransform;
        Toggle gbl;
        Foldout foldout;
        public ChannelItem(string name, Color backgroundColor, TransformEditor transformEditor, UVAnimEditor uvanimEditor, S4Animation animation)
        {
            _transformEditor = transformEditor;
            _uvanimEditor = uvanimEditor;
            gbl = new Toggle();
            foldout = new Foldout();
            var pos = new Toggle();
            var rot = new Toggle();
            var sca = new Toggle();
            var alp = new Toggle();

            gbl.SetValueWithoutNotify(true);
            foldout.text = name;
            foldout.value = false;

            pos.text = "Position"; rot.text = "Rotation"; sca.text = "Scale"; alp.text = "Alpha";
            pos.style.paddingLeft = rot.style.paddingLeft = sca.style.paddingLeft = alp.style.paddingLeft = 30;

            pos.SetValueWithoutNotify(false);
            rot.SetValueWithoutNotify(false);
            sca.SetValueWithoutNotify(false);
            alp.SetValueWithoutNotify(false);

            pos.style.backgroundColor = rot.style.backgroundColor = sca.style.backgroundColor = alp.style.backgroundColor = bg;

            foldout.Add(pos);
            foldout.Add(rot);
            foldout.Add(sca);
            foldout.Add(alp);

            gbl[0].style.flexGrow = 0f;
            gbl[0][0].style.alignSelf = Align.FlexStart;
            Add(gbl);
            Add(foldout);
            style.flexDirection = FlexDirection.Row;

            style.backgroundColor = backgroundColor;

            gbl.RegisterValueChangedCallback((evnt) => { BoneChannelGlobalCallback(evnt, animation.TransformKeyData); });

            pos.RegisterValueChangedCallback((evnt) =>
            {
                transformEditor.SetChannel(animation.TransformKeyData, evnt.newValue, TransformChannel.Channel.Position);
                evnt.StopPropagation();
            });
            rot.RegisterValueChangedCallback((evnt) =>
            {
                transformEditor.SetChannel(animation.TransformKeyData, evnt.newValue, TransformChannel.Channel.Rotation);
                evnt.StopPropagation();
            });
            sca.RegisterValueChangedCallback((evnt) =>
            {
                transformEditor.SetChannel(animation.TransformKeyData, evnt.newValue, TransformChannel.Channel.Scale);
                evnt.StopPropagation();
            });
            alp.RegisterValueChangedCallback((evnt) =>
            {
                transformEditor.SetChannel(animation.TransformKeyData, evnt.newValue, TransformChannel.Channel.Alpha);
                evnt.StopPropagation();
            });

        }


        void BoneChannelGlobalCallback(ChangeEvent<bool> e, TransformKeyData tkd)
        {
            Foldout foldout = ((VisualElement)(e.target)).parent[1] as Foldout;
            Toggle pos = foldout[0] as Toggle;
            Toggle rot = foldout[1] as Toggle;
            Toggle sca = foldout[2] as Toggle;
            Toggle alp = foldout[3] as Toggle;
            if (e.newValue)
            {
                pos.SetEnabled(true);
                rot.SetEnabled(true);
                sca.SetEnabled(true);
                alp.SetEnabled(true);
                _transformEditor.SetAllChannels(tkd, pos.value);
            }
            else
            {
                pos.SetEnabled(false);
                rot.SetEnabled(false);
                sca.SetEnabled(false);
                alp.SetEnabled(false);
                _transformEditor.SetAllChannels(tkd, false);
            }
            e.StopPropagation();
        }

        void UvGlobalCallback(ChangeEvent<bool> e)
        {

        }

        void SetTransformEnabled()
        {
            displayingTransform = true;

            gbl.RegisterValueChangedCallback((evnt) => { BoneChannelGlobalCallback(evnt, null); });
            foldout.SetEnabled(true);
        }

        void SetUVEnabled()
        {
            displayingTransform = false;

            gbl.RegisterValueChangedCallback((evnt) => { UvGlobalCallback(evnt); });

            foldout.value = false;
            foldout.SetEnabled(false);
        }
    }
}