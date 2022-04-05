# Animator ConstantFields Generator
A unity script that generates constant fields for animatorControllers in a unity project

Install
Go to ```Package Manager``` -> ```Add Package from git URL``` 
Paste the following ```https://github.com/BarelyAStudio/AnimatorConstantFieldsGenerator.git```

# Sample Generated Code
```cs
namespace GeneratedNamespace
{
	public class NewAnimatorController
	{
		/// <summary>
		/// The animation clip names that are attached to all animator states. <br /><br />
		/// Majority of the API in the <see cref="Animator"/> uses the state rather than the animation clip. <br />
		/// When you create a state for the first time, it's often the case that the the State.name == Animation.name. <br />
		/// So <see cref="Animator.Play(string)"/> works when you pass in the name of the animation clip. <br /><br />
		/// If your animation isn't playing from <see cref="Animator.Play(string)"/>, <br />
		/// then you have set the name of the State differently than your Animation. <br />
		/// Pass in any of the fields in the States struct within the <see cref="Layers"/> struct instead. <br />
		/// </summary>
		public readonly struct Animations
		{
			public const string NEW_ANIMATION = "New Animation";
		}
		public readonly struct Layers
		{
			/// <summary>
			/// One of the <see cref="Layers"/> in the <see cref="New Animator Controller"/> Animator.
			/// </summary>
			public readonly struct Base_Layer
			{
				public const string Name = "Base Layer";
				public const int Index = 0;
				/// <summary>
				/// The states in the <see cref="Layers.Base_Layer"/>.
				/// </summary>
				public readonly struct States
				{
					public const int DEFAULT_STATE = NEW_ANIMATION;
					public const int NEW_ANIMATION = -1996450064;
				}
			}
		}
	}
}
```
