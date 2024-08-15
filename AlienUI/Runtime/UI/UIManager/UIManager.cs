using AlienUI.Models;
using System.Collections.Generic;
using UnityEngine;

namespace AlienUI.UIElements
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField]
        private RectTransform m_uiRoot;
        [SerializeField]
        private Engine m_engine;

        public Engine Engine => m_engine;

        private RectTransform m_hudRoot;
        private RectTransform m_windowRoot;
        private RectTransform m_floatRoot;

        List<Settings.AmlResouces> m_hudSeq = new List<Settings.AmlResouces>();
        List<ViewModel> m_hudSeq_vm = new List<ViewModel>();
        HUD m_currentHUD = null;

        Dictionary<Settings.AmlResouces, HashSet<Window>> m_openedWindowsMap = new Dictionary<Settings.AmlResouces, HashSet<Window>>();
        Dictionary<Window, Settings.AmlResouces> m_openedWindows = new Dictionary<Window, Settings.AmlResouces>();

        private static UIManager m_instance;
        public static UIManager Instance
        {
            get
            {
                if (Application.isPlaying) return m_instance;
                else
                {
                    return GameObject.FindObjectOfType<UIManager>();
                }
            }
            private set
            {
                m_instance = value;
            }
        }

        private void Awake()
        {
            Instance = this;

            if (gameObject.transform.parent == null) DontDestroyOnLoad(gameObject);
            if (m_engine.transform.parent == null) DontDestroyOnLoad(m_engine.gameObject);
            if (m_uiRoot.transform.parent == null) DontDestroyOnLoad(m_uiRoot.gameObject);

            m_hudRoot = createUINode("[HUD]");
            m_windowRoot = createUINode("[Window]");
            m_floatRoot = createUINode("[Float]");
        }

        public T OpenWindow<T>(Settings.AmlResouces amlRes, ViewModel viewModel = null) where T : Window
        {
            if (amlRes == null) return null;
            if (amlRes.Aml == null) return null;

            if (!typeof(T).IsAssignableFrom(amlRes.AssetType)) return null;

            var newUI = m_engine.CreateUI(amlRes.Aml, m_windowRoot, viewModel);

            if (!m_openedWindowsMap.ContainsKey(amlRes))
                m_openedWindowsMap[amlRes] = new HashSet<Window>();

            m_openedWindowsMap[amlRes].Add(newUI as Window);
            m_openedWindows[newUI as Window] = amlRes;

            newUI.OnFocusChanged += NewUI_OnFocusChanged;
            newUI.OnClose += NewUI_OnClose;

            m_engine.Focus(newUI);

            return newUI as T;
        }

        public T OpenHUD<T>(Settings.AmlResouces amlRes, ViewModel viewModel = null) where T : HUD
        {
            if (amlRes == null) return null;
            if (amlRes.Aml == null) return null;

            if (!typeof(T).IsAssignableFrom(amlRes.AssetType)) return null;

            var index = m_hudSeq.IndexOf(amlRes);
            if (index == -1) //未在队列中出现过的HUD
            {
                if (m_currentHUD != null)
                {
                    m_currentHUD.Close();
                    m_currentHUD.OnFocusChanged -= NewUI_OnFocusChanged;
                    m_currentHUD.OnClose -= NewUI_OnClose;
                    m_currentHUD = null;
                }
                m_hudSeq.Add(amlRes);
                m_hudSeq_vm.Add(viewModel);
                m_currentHUD = m_engine.CreateUI(amlRes.Aml, m_hudRoot, viewModel) as HUD;
            }
            else if (index == m_hudSeq.Count - 1) //请求打开的HUD为当前HUD
            {
                return m_currentHUD as T;
            }
            else //在队列中打开过的HUD,将之后的HUD移除队列
            {
                m_hudSeq.RemoveRange(index + 1, m_hudSeq.Count - index - 1);
                m_hudSeq_vm.RemoveRange(index + 1, m_hudSeq.Count - index - 1);
                if (m_currentHUD != null)
                {
                    m_currentHUD.Close();
                    m_currentHUD.OnFocusChanged -= NewUI_OnFocusChanged;
                    m_currentHUD.OnClose -= NewUI_OnClose;
                    m_currentHUD = null;
                }
                m_currentHUD = m_engine.CreateUI(amlRes.Aml, m_hudRoot, viewModel) as HUD;
            }

            m_currentHUD.OnFocusChanged += NewUI_OnFocusChanged;
            m_currentHUD.OnClose += NewUI_OnClose;

            m_engine.Focus(m_currentHUD);

            return m_currentHUD as T;
        }

        public bool CanHUDBack(HUD hud)
        {
            return m_currentHUD == hud && m_hudSeq.Count > 1;
        }

        public void HUDBack(HUD hud)
        {
            if (!CanHUDBack(hud)) return;

            m_currentHUD.Close();
            m_hudSeq.RemoveAt(m_hudSeq.Count - 1);
            m_hudSeq_vm.RemoveAt(m_hudSeq.Count - 1);

            var amlRes = m_hudSeq[m_hudSeq.Count - 1];
            var vm = m_hudSeq_vm[m_hudSeq.Count - 1];

            OpenHUD<HUD>(amlRes, vm);
        }

        private void NewUI_OnClose(object sender, Events.OnCloseEvent e)
        {
            if (sender is Window wnd)
            {
                RemoveWindow(wnd);
            }
        }

        private void NewUI_OnFocusChanged(UIElement ui)
        {
            if (ui is Window wnd && wnd.Focused && HasWindow(wnd))
            {
                wnd.Rect.SetAsLastSibling();
            }
        }

        private void RemoveWindow(Window wnd)
        {
            var amlRes = GetWindowResource(wnd);
            if (amlRes == null) return;

            if (!m_openedWindowsMap.TryGetValue(amlRes, out var windowList)) return;

            windowList.Remove(wnd);
            m_openedWindows.Remove(wnd);

            wnd.OnFocusChanged -= NewUI_OnFocusChanged;
            wnd.OnClose -= NewUI_OnClose;
        }

        private Settings.AmlResouces GetWindowResource(Window wnd)
        {
            m_openedWindows.TryGetValue(wnd, out Settings.AmlResouces resouces);
            return resouces;
        }

        private bool HasWindow(Window wnd)
        {
            return GetWindowResource(wnd) != null;
        }

        private RectTransform createUINode(string name)
        {
            var nodeGo = new GameObject(name);
            var nodeRect = nodeGo.AddComponent<RectTransform>();
            nodeRect.SetParent(m_uiRoot);

            nodeRect.anchorMin = new Vector2(0, 0);
            nodeRect.anchorMax = new Vector2(1, 1);
            nodeRect.pivot = new Vector2(0.5f, 0.5f);
            nodeRect.sizeDelta = Vector2.zero;
            nodeRect.anchoredPosition3D = Vector3.zero;
            nodeRect.localScale = Vector2.one;

            return nodeRect;
        }
    }
}
