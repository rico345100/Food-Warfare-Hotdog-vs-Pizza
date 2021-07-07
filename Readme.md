# Food Warfare: Hotdog vs Pizza
## What the heck is this?
A simple unity project for R&D multiplayer with Photon Unity Networking 2.

## How do I run?
Just for singleplayer, just open it up in Unity and play or build.

To play multiplayer, locate to PhotonServerSettings in Assets/Photon/PhotonUnityNetworking/Resources.

You need an App ID and you can earn it from Photon Dashboard, 20 CCU is free.
https://dashboard.photonengine.com/

After setting the App ID, update Fixed Region. It's set to "kr" by default because I live in South Korea.
You can just empty this to pick the best region by Photon Network automatically.

If you can't connect to Photon Network after build, try disabling your firewall and try it again.

Don't forget to bake the lightmaps!

## Requirements
- Unity 2019.4.3f1

## FAQ
### Why you are instantiating objects in different ways? What's the difference between InGamePoolManager.Instance.Get and PhotonNetwork.Instantiate?
Photon supports object pooling by assigning the existing pool system manually. Here's the code in InGamePoolManager that register my own custom implemented object pool to use in Photon:

```csharp
using Photon.Pun;

public class InGamePoolManager : LocalSingleton<InGamePoolManager>, IPunPrefabPool {
    protected override void OnInit() {
            PhotonNetwork.PrefabPool = this;
            ...
    }
}
```

After assigning prefab pool, using PhotonNetwork.Instantiate or similar methods will use my InGamePoolManager.Instance.Get() instead of Unity's built-in Instantiate method.

However, using PhotonNetwork.Instantiate will trigger the other clients to make the same object, and also requires instantiating object must have a PhotonView component.

The stuff like particle system or sound effect does not have to be perfectly synchronized through the network, instead, you can just notify the other clients that there was an explosion in the position of somewhere.

The other client who received that signal will create special effects by fetching an object from their own pool.

# Buy me a coffee!
If you enjoyed this project, you can buy me a coffee so that I can make more of this kind of stuff.

[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=PVXTU5FJNBLDS)

# 푸드 워페어: 핫도그 vs 피자
## 이게 뭔가요?
포톤 2 연구용으로 만든 간단한 멀티플레이어 게임 프로젝트입니다.

## 어떻게 실행하나요?
싱글플레이의 경우 그냥 유니티에서 열고 플레이하시거나 빌드에서 플레이하시면 됩니다.

멀티플레이어는 Assets/Photon/PhotonUnityNetworking/Resources에 있는 PhotonServerSettings에 App ID를 설정해줘야 합니다.

App ID는 포톤 대시보드에서 20 CCU 사용까지 무료로 발급받을 수 있습니다.
https://dashboard.photonengine.com/

App ID 설정 후, Fixed Region을 변경해주세요. 저는 한국에 살기 때문에 "kr"로 해봤지만, 공백으로 설정하면 포톤이 알아서 최적의 리젼으로 설정해준다고 합니다.

빌드 후 포톤 네트워크에 연결이 안된다면, 방화벽을 끄고 시도해보시기 바랍니다.

라이트맵을 베이크하는걸 잊지 마세요!

## 요구사항
- 유니티 2019.4.3f1

## FAQ
### 왜 오브젝트를 InGamePoolManager.Instance.Get과 PhotonNetwork.Instantiate를 둘 다 사용해서 만들죠? 차이점이 뭔가요?
포톤은 사용자가 구축한 풀링 시스템을 사용할 수 있도록 지원합니다. 아래 코드는 InGamePoolManager에서 이를 처리하는 로직입니다.

```csharp
using Photon.Pun;

public class InGamePoolManager : LocalSingleton<InGamePoolManager>, IPunPrefabPool {
    protected override void OnInit() {
            PhotonNetwork.PrefabPool = this;
            ...
    }
}
```

Prefab Pool에 제가 만든 풀을 할당해주면, PhotonNetwork.Instantiate와 같은 함수를 쓸때, 유니티 내장 Instantiate 대신 제가 만든 InGamePoolManager.Instance.Get을 사용합니다.

하지만 PhotonNetwork.Instantiate를 사용하면 다른 클라이언트들에게도 똑같은 오브젝트를 생성하게 하는데요, 특히 풀링된 오브젝트가 PhotonView 컴포넌트를 반드시 소지해야되는 문제도 있습니다.

시각 효과나 음향 효과 같은 경우는 완벽한 타이밍에 동기화할 필요는 없으므로, 그냥 다른 클라이언트들에게 어떤 위치에서 폭발이 발생했다 라는 신호 정도만 전달하면 됩니다.

이 신호를 받은 다른 클라이언트들은 풀에서 폭발 효과 오브젝트를 빼와서 그 위치에 옮겨놓기만 하면 추가적인 데이터 전송이 전혀 필요가 없죠.


# 커피 한잔의 후원!
이 프로젝트가 도움이 되었다면, 간단하게나마 커피값이라도 후원해주신다면 감사하겠습니다.

[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=PVXTU5FJNBLDS)
