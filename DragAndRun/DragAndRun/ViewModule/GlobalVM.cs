using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DragAndRun.ViewModule
{
    class GlobalVM
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                this.PropertyChanged(this, e);
            }
        }
        private static GlobalVM _instance;
        public static GlobalVM Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GlobalVM();
                return _instance;
            }
        }

        private bool _encodeAndroid;
        public bool EncodeAndroid
        {
            get { return _encodeAndroid; }
            set { _encodeAndroid = value; OnPropertyChanged("EncodeAndroid"); }
        }
        
        public enum PackType
        {
            None,
            Pack_All,
            Pack_Sub,
        }
        private PackType _type;
        public PackType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        //static public SynchronizationContext syncContext;
        //public method
        public void updateDescription(string msg)
        {
            //syncContext.Post(_updateDescription, (object)msg);
            if (GlobalVM.Instance.Type == GlobalVM.PackType.Pack_All)
            {
                PackageAllVM.Instance.updateDescription(msg);
            }
            else if (GlobalVM.Instance.Type == GlobalVM.PackType.Pack_Sub)
            {
                PackageSubVM.Instance.updateDescription(msg);
            }
        }
        static public void _updateDescription(object msg)
        {
            if (GlobalVM.Instance.Type == GlobalVM.PackType.Pack_All)
            {
                PackageAllVM.Instance.updateDescription((string)msg);
            }
            else if (GlobalVM.Instance.Type == GlobalVM.PackType.Pack_Sub)
            {
                PackageSubVM.Instance.updateDescription((string)msg);
            }
        }
    }
}
