
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

using Object = UnityEngine.Object;

[ExecuteInEditMode]
public class PropertyMaster : MonoBehaviour, IExposedPropertyTable, ISerializationCallbackReceiver {
    internal static readonly HashSet<Type> componentTypes = new HashSet<Type>();

    public enum UpdateMode {
        Automatic,
        Manual
    }

    public UpdateMode updateMode;

    [Space(9)]
    public bool updateVolumes;
    public Transform volumeTrigger;
    public LayerMask volumeLayerMask = 0;

    int _lastFrameCount = -1;

    [Serializable]
    struct ExposedReferenceData {
        public PropertyName name;
        public Object value;
    }

    [SerializeField, HideInInspector]
    List<ExposedReferenceData> _exposedReferenceList = new List<ExposedReferenceData>();
    Dictionary<PropertyName, Object> _exposedReferenceTable = new Dictionary<PropertyName, Object>();

    protected void Start() {
        // Need to do this once, to flush out any remaining volumes after a level load,
        // or our update below will use stale data in VolumeManager which is only updated
        // by HDRP just before rendering.
        if (!updateVolumes)
            VolumeManager.instance.Update(null, -1);
    }

    protected void LateUpdate() {
        if (updateMode == UpdateMode.Automatic)
            UpdateProperties();
    }

    public void UpdateProperties() {
        var lastFrameCount = _lastFrameCount;
        _lastFrameCount = Time.frameCount;

        if (lastFrameCount == Time.frameCount) {
            Debug.LogWarning("PropertyMaster updated more than once this frame", this);
            return;
        }

        var manager = VolumeManager.instance;
        var stack = manager.stack;

        if (updateVolumes && volumeTrigger && volumeLayerMask != 0)
            manager.Update(volumeTrigger, volumeLayerMask);

        foreach (var type in componentTypes) {
            var component = (PropertyVolumeComponentBase) stack.GetComponent(type);

            if (component.active)
                component.OverrideProperties(this);
        }
    }

    public void OnBeforeSerialize() {
        _exposedReferenceList = new List<ExposedReferenceData>();

        foreach (var i in _exposedReferenceTable)
            _exposedReferenceList.Add(new ExposedReferenceData {name = i.Key, value = i.Value});
    }

    public void OnAfterDeserialize() {
        _exposedReferenceTable = new Dictionary<PropertyName, Object>();

        foreach (var i in _exposedReferenceList)
            _exposedReferenceTable.Add(i.name, i.value);
    }

    public void ClearReferenceValue(PropertyName name) {
        _exposedReferenceTable.Remove(name);
    }

    public Object GetReferenceValue(PropertyName name, out bool valid) {
        Object value;
        valid = _exposedReferenceTable.TryGetValue(name, out value);
        return value;
    }

    public void SetReferenceValue(PropertyName name, Object value) {
        _exposedReferenceTable[name] = value;
    }
}

