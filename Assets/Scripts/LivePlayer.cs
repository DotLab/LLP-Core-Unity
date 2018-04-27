using UnityEngine;

using LoveLivePractice.Api;

public class LivePlayer : MonoBehaviour {
	sealed class FResource<T> : System.IDisposable where T : UnityEngine.Object {
		T _asset;
		public T asset {
			get {
				if (_asset != null) return _asset;
				throw new System.NullReferenceException();
			}
		}

		public FResource(string path) {
			_asset = Resources.Load<T>(path);
			if (_asset == null) throw new System.NullReferenceException();
		}

		public void Dispose() {
			Resources.UnloadAsset(_asset);
			_asset = null;
		}
	}

	public string liveName;
	public AudioSource source;

	public ApiLive live;
	public ApiLiveMap map;
	public AudioClip bgm;

	public double bufferInterval, startTime;
	public int index;

	void OnValidate() {
		source = GetComponent<AudioSource>();
	}

	void Start() {
		using (var liveAsset = new FResource<TextAsset>("lives/" + liveName)) {
			live = JsonUtility.FromJson<ApiLiveResponse>(liveAsset.asset.text).content;
		}

		using (var mapAsset = new FResource<TextAsset>("maps/" + live.map_path.Replace(".json", ""))) {
			map = JsonUtility.FromJson<ApiLiveMap>(mapAsset.asset.text);
			System.Array.Sort(map.lane);
		}

		bgm = Resources.Load<AudioClip>("bgms/" + live.bgm_path.Replace(".mp3", ""));

		source.clip = bgm;
		source.Play();

		startTime = AudioSettings.dspTime;
	}

	void Update() {
		double time = AudioSettings.dspTime - startTime;
		double bufferTime = time + bufferInterval;

		while (index < map.lane.Length && map.lane[index].starttime / 1000.0 <= bufferTime) {
			var note = map.lane[index];
			Debug.Log(note.lane);

			index += 1;
		}
	}
}
