// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using UnityEngine;

namespace Facebook.Workrooms.SerializeReferencePickerExample {

  [CreateAssetMenu(
    menuName = "Testing/SerializeReferencePickerExample",
    fileName = "SerializeReferencePickerExample"
  )]
  public class SerializeReferencePickerExample : ScriptableObject {
    [SerializeReference, SerializeReferencePicker]
    public AbstractClass Object;

    [SerializeReference, SerializeReferencePicker]
    public AbstractClass[] Objects;

    [SerializeReference, SerializeReferencePicker]
    public IInterface[] Interfaces;
  }

  [Serializable]
  public abstract class AbstractClass {
    public int PropertyFromAbstractClass;
  }

  [Serializable]
  public class Child1 : AbstractClass {
    public int PropertyFromChild1;
  }

  [Serializable]
  public class Child2 : AbstractClass {
    public int PropertyFromChild2;
    public int PropertyFromChild2Again;
  }

  public abstract class AbstractChild : AbstractClass {
    public int Nope;
  }

  public class TemplateChild<T> : AbstractClass {
    public T Nope;
  }

  public interface IInterface {
  }

  [Serializable]
  public class Implementation1 : IInterface {
    public int Implementation1a;
  }

  [Serializable]
  public class Implementation2 : IInterface {
    public int Implementation2a;
    public int Implementation2b;
  }
}
