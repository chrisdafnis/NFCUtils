using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;

namespace com.touchstar.chrisd.nfcutils
{
    [Activity(Label = "SingleFragmentActivity")]
    public abstract class SingleFragmentActivity : AppCompatActivity
    {
        private readonly string TAG = "SingleFragmentActivity";
        protected abstract Fragment CreateFragment();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            SetContentView(Resource.Layout.activity_fragment);

            //fm = SupportFragmentManager;
            //Fragment fragment = FragmentManager.FindFragmentById(Resource.Id.fragment_container);
            Fragment fragment = FragmentManager.FindFragmentByTag(TAG);
            if (fragment == null)
            {
                fragment = CreateFragment();
                FragmentManager.BeginTransaction()
                    .Add(Resource.Id.fragment_container, fragment)
                    .Commit();
            }
        }

        public void ClearBackStack(bool includeLast)
        {
            DebugBackstack();

            int backstackCount;

            Android.Support.V4.App.FragmentManager fm = SupportFragmentManager;

            backstackCount = fm.BackStackEntryCount;

            if (backstackCount == 0)
                return;

            if (includeLast)
                for (int i = 0; i < backstackCount; i++)
                    fm.PopBackStackImmediate();
            else
                for (int i = 0; i < backstackCount - 1; i++)
                    fm.PopBackStackImmediate();

            DebugBackstack();

        }

        public void PopBackStack(int howMany)
        {
            DebugBackstack();

            if (howMany < 1)
                return;

            int backstackCount;

            //FragmentManager fm = SupportFragmentManager;

            backstackCount = FragmentManager.BackStackEntryCount;

            if (backstackCount == 0)
                return;

            if (howMany > backstackCount)
                howMany = backstackCount;

            for (int i = 0; i < howMany; i++)
                FragmentManager.PopBackStackImmediate();

            DebugBackstack();

        }
        /// <summary>
        /// Pops the back stack of all entries upto but not including the tag
        /// </summary>
        /// <param name="tag"></param>
        public void PopBackStackToTag(string tag)
        {

            int backstackCount;

            Android.Support.V4.App.FragmentManager fm = SupportFragmentManager;

            backstackCount = fm.BackStackEntryCount;

            if (backstackCount == 0)
                return;


            for (int i = backstackCount; i > 0; i--)
            {
                if (fm.GetBackStackEntryAt(i - 1).Name == tag)
                    break;
                fm.PopBackStackImmediate();
            }
        }

        public void DebugBackstack()
        {
            //FragmentManager fm = SupportFragmentManager;

            int i;
            FragmentManager.IBackStackEntry back;

            for (i = 0; i < FragmentManager.BackStackEntryCount; i++)
            {
                back = FragmentManager.GetBackStackEntryAt(i);
                //Log.Info(TAG, string.Format("Back Stack {0} {1} of {2}", back.Name, i + 1, fm.BackStackEntryCount));
            }
        }

        public string GetFragmentOnStack()
        {
            //FragmentManager fm = SupportFragmentManager;

            FragmentManager.IBackStackEntry back;

            if (FragmentManager.BackStackEntryCount == 0)
                return null;

            back = FragmentManager.GetBackStackEntryAt(0);

            return back.Name;
        }

        public override void OnBackPressed()
        {
            //base.OnBackPressed();
        }
    }
}