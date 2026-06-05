# Inflation Bike Pump
A member of the now discontinued Discord Server suggested adding a Bike Pump to the world. Even though it wasn't the subject of the world, It was something that could fit in the world because it did have a Pool in the world already.

In order to set this up in your world, You need a invisible pickup for the handle, A transform (or seperate mesh object), the base mesh, and a tube mesh. Pair it with some sound effects and that should be it.
If you want the pump's tube to be animated, you will need the pairing `BikePumpAnimationHandler.cs` script. It animates the tube's blendshapes without needing a unnessesary Animator, which may cause a hit on Performance on Standalone, or so I heard from a few guides I read.

In my original implementation from my world, I used a paid prefab (JetDog's Avatar Scale Prefabs) to grow the player, so I didn't have to learn to script player height changes, which was a massive time save for me. The `targetUdonBehavior` is triggered for that purpose. You can change what event is called within the inspector. Reuse this for your own use, whether it be for something custom, or for the same scale system I used within Paradise Park.

The Invisible Pickup is used for tracking the position of the grip. It's used to let the player interact and also clamp the grip mesh so it doesn't visually look or work incorrectly to what a real bike pump would look like. Do note, when the pickup for the handle is dropped, it returns to position 0 so either parent the pickup under an object or account for it another way. In my implementation, I believe I went for the parented option. 

You need a Trigger Collider attatched to determine if the player gets effected or not. Either call the trigger events externally by a script at the end to act as a sort of relay, or place the trigger where the end of the tube would be. 

In the old pre-dynamics implementation (i.e. when they added Avatar Dynamics support in worlds in VRChat), I used regular Unity Physics to give the tubes physics. I don't recommend this and is not in the world anymore, as Physbones works more effectively, at the cost of Flatscreen player unable to grab. For that, if you'd like Flatscreen (Non-VR) Players to grab physbones, Please upvote [this requested feature](https://feedback.vrchat.com/feature-requests/p/making-physbones-also-grabbable-on-desktop-permanently-or-as-a-setting) to hopefully get VRChat to add grabbing support for those who don't have VR to do so.
