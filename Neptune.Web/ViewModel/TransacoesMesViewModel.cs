﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Neptune.Models;

namespace Neptune.Web.ViewModel
{
    public class TransacoesMesViewModel
    {
        public decimal SaldoUltimoDiaMesAnterior { get; private set; }
        public string UltimoDiaMesAnterior { get { return _ultimoDiaMesAnterior.ToString("dd/MM/yyyy"); } }
        public List<DiaViewModel> Dias { get; private set; } = new();

        public List<ContaViewModel> Contas { get; private set; } = new List<ContaViewModel>();

        public int Ano;
        public int Mes;

        private DateTime _ultimoDiaMesAnterior
        {
            get
            {
                return new DateTime(Ano, Mes, 1).AddDays(-1);
            }
        }

        public TransacoesMesViewModel(int ano, int mes, IEnumerable<Transacao> transacoesModel, decimal saldoUltimoDiaMesAnterior, List<Conta> contasModel)
        {
            Ano = ano;
            Mes = mes;

            transacoesModel.ToList().Sort((x, y) => x.Data.CompareTo(y.Data));

            SaldoUltimoDiaMesAnterior = saldoUltimoDiaMesAnterior;

            var dias = transacoesModel
                .GroupBy(item => new {item.Data.Month, item.Data.Day}).Select(x => x.First())
                .Select(d => d.Data).ToList();

            var saldoDiaAnterior = 0M;
            for (var index = 0; index < dias.Count; index++)
            {
                var dia = dias[index];
                var transacoesDia = transacoesModel.Where(x => x.Data.Day == dia.Day);

                if (index == 0)
                {
                    var diaViewModel = new DiaViewModel(dia, transacoesDia, saldoUltimoDiaMesAnterior);
                    saldoDiaAnterior = diaViewModel.ObterSaldoDoDia();
                    Dias.Add(diaViewModel);
                }
                else
                {
                    var diaViewModel = new DiaViewModel(dia, transacoesDia, saldoDiaAnterior);
                    saldoDiaAnterior = diaViewModel.ObterSaldoDoDia();
                    Dias.Add(diaViewModel);
                }
            }

            contasModel.ForEach(x => Contas.Add(new ContaViewModel(x.Id, x.Nome, true)));
        }

        public void AdicionarTransacao(TransacaoViewModel transacaoViewModel)
        {
            var dia = Dias.FirstOrDefault(x => (x.Data.Day == transacaoViewModel.Data.Day &&
                x.Data.Month == transacaoViewModel.Data.Month &&
                x.Data.Year == transacaoViewModel.Data.Year));

            if (dia == null)
            {
                var diaAnterior = Dias.FirstOrDefault(x => (x.Data.Day < transacaoViewModel.Data.AddDays(-1).Day &&
                                                            x.Data.Month == transacaoViewModel.Data.Month &&
                                                            x.Data.Year == transacaoViewModel.Data.Year));

                var saldoDoDiaAnterior = diaAnterior.ObterSaldoDoDia();
                var transacoes = new List<Transacao>() { new Transacao(transacaoViewModel.Id, transacaoViewModel.Data, transacaoViewModel.Descricao, transacaoViewModel.Valor, transacaoViewModel.ContaId) };

                var novoDia = new DiaViewModel(transacaoViewModel.Data, transacoes, saldoDoDiaAnterior);

                Dias.Add(novoDia);                
            }
            else
                dia.AdicionarTransacao(transacaoViewModel);

            Dias.Sort((x, y) => x.Data.CompareTo(y.Data));
        }

        public int ObterMesAnterior()
        {
            return new DateTime(Ano, Mes, 1).AddMonths(-1).Month;
        }

        public int ObterMesSeguinte()
        {
            return new DateTime(Ano, Mes, 1).AddMonths(1).Month;
        }

        public int ObterAnoDoMesAnterior()
        {
            return new DateTime(Ano, Mes, 1).AddMonths(-1).Year;
        }

        public int ObterAnoDoMesSeguinte()
        {
            return new DateTime(Ano, Mes, 1).AddMonths(1).Year;
        }
    }
}
