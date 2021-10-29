﻿using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Neptune.Models;
using Neptune.Web.ViewModel;
using System;

namespace Neptune.Web.Services
{
    public class TransacaoService : ITransacaoService
    {
        public HttpClient HttpClient;

        public TransacaoService(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public async Task<TransacoesViewModel> ObterTransacoesViewModel(DateTime data)
        {
            var transacoes = await HttpClient.GetFromJsonAsync<List<Transacao>>("/api/transacao");
            var contaModel = await HttpClient.GetFromJsonAsync<Conta>("/api/conta/1"); //TODO: tirar essa ContaId = 1 hardcoded

            transacoes.Sort((x, y) => x.Data.CompareTo(y.Data));
            //var diasViewModel = new List<DiaViewModel>();
            var transacoesMesPassadoPraTras = transacoes.Where(x => x.Data.Month < data.Month && x.Data.Year <= data.Year);
            var saldoMesAnterior = contaModel.SaldoInicial - transacoesMesPassadoPraTras.Where(x => x.ContaId == contaModel.Id).Sum(x => x.Valor);

            //for (int i = 0; i < transacoes.Count; i++)
            //{
                //var transacoesDia = transacoes.Where(x => x.Data.Day == data.Day && x.Data.Month == data.Month && x.Data.Year == data.Year);
                //var diaViewModel = new DiaViewModel(data, transacoesDia, saldoMesAnterior + transacoesDia.Sum(x => x.Valor));
                //diasViewModel.Add(diaViewModel);
            //}

            var transacoesModelMes = transacoes.Where(x => x.Data.Month == data.Month);

            var transacoesViewModel = new TransacoesViewModel(transacoesModelMes, saldoMesAnterior);

            return transacoesViewModel;
        }

        public async Task<Transacao> ObterTransacao(int id)
        {
            return await HttpClient.GetFromJsonAsync<Transacao>($"/api/transacao/{id}");
        }

        public async Task<Transacao> EditarTransacao(int id, Transacao transacao)
        {
            var response = await HttpClient.PutAsJsonAsync($"/api/transacao/{id}", transacao);
            return await response.Content.ReadFromJsonAsync<Transacao>();
        }

        public async Task<TransacaoViewModel> NovaTransacao(NovaTransacaoViewModel novaTransacaoViewModel)
        {
            var transacao = new Transacao
            {
                Data = novaTransacaoViewModel.Data,
                Descricao = novaTransacaoViewModel.Descricao,
                Valor = novaTransacaoViewModel.Valor,
                ContaId = novaTransacaoViewModel.ContaId
            };

            var response = await HttpClient.PostAsJsonAsync("/api/transacao", transacao);
            var transacaoModel = await response.Content.ReadFromJsonAsync<Transacao>();

            return new TransacaoViewModel(transacaoModel);
        }
    }
}