using JOJO.Mes.Const;
using JOJO.Mes.Model;
using System;
using System.Threading.Tasks;

namespace JOJO.Mes.Client
{
    internal class DefaultMes : IMesSend
    {
        MesModel MesModel { get;}=new MesModel();
        public Task<MesProcessData> AlarmInformation(string message, int Level)
        {
            return Task.FromResult(new MesProcessData() {   MesValue = MesModel.SetResultOKEnable(false, false) });
        }

        public Task<MesProcessData> CancelAlarmInformation(string message)
        {
            return Task.FromResult(new MesProcessData() { MesValue = MesModel.SetResultOKEnable(false, false) });
        }

        public Task<MesProcessData> CheckBoard(MesDynamic dynamic)
        {
            return Task.FromResult(new MesProcessData() { MesValue = MesModel.SetResultOKEnable(false, false) });
        }

        public void CloseMes()
        {
            return ;
        }
        public bool MesEnable()
        {
            return false;
        }

        public Task<MesProcessData> MesLogin(MesDynamic dynamic)
        {
            return Task.FromResult(new MesProcessData() { MesValue = MesModel.SetResultOKEnable(false, false) });
        }

        public Task<MesProcessData> OutBoard(MesDynamic dynamic)
        {
            return Task.FromResult(new MesProcessData() { MesValue = MesModel.SetResultOKEnable(false, false) });
        }

        public Task<MesProcessData> ProcessParameters()
        {
            return Task.FromResult(new MesProcessData() { MesValue = MesModel.SetResultOKEnable(false, false) });
        }

        public Task<MesProcessData> ProcessStop(MesEnum.MachineState on)
        {
            return Task.FromResult(new MesProcessData() { MesValue = MesModel.SetResultOKEnable(false, false) });
        }

        public Task<MesProcessData> RemovePCB(MesDynamic data)
        {
            return Task.FromResult(new MesProcessData() { MesValue = MesModel.SetResultOKEnable(false, false) });
        }

        public Task<MesProcessData> Result(MesDynamic dynamic)
        {
            return Task.FromResult(new MesProcessData() { MesValue = MesModel.SetResultOKEnable(false, false) });
        }

        public Task<MesProcessData> SwitchPrograms()
        {
            return Task.FromResult(new MesProcessData() { MesValue = MesModel.SetResultOKEnable(false, false) });
        }

        Task<MesProcessData> IMesSend.Dynamic(MesDynamic dynamic)
        {
            return Task.FromResult(new MesProcessData() { MesValue = MesModel.SetResultOKEnable(false, false) });
        }
    }
}
