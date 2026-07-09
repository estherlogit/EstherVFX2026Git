// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System.Collections.Generic;
using Facebook.Workrooms.MixedReality;

namespace Tests.UnitTests.MixedReality {
  public class FakePlayerPrefsRepository : IPlayerPrefsRepository {

    private Dictionary<string, string> store_ = new Dictionary<string, string>();

    public string GetValue(string key) {
      return store_[key];
    }

    public void SetValue(string key, string value) {
      store_[key] = value;
    }

    public bool HasKey(string key) {
      return store_.ContainsKey(key);
    }

    public void DeleteKey(string key) {
      if (store_.ContainsKey(key)) {
        store_.Remove(key);
      }
    }

  }
}
