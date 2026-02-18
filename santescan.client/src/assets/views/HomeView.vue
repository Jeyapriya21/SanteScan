<template>
  <div class="min-h-screen bg-slate-50 font-sans text-slate-900">
    <AppHeader />

    <main class="max-w-6xl mx-auto px-4 py-8">
      <div class="grid grid-cols-1 lg:grid-cols-12 gap-8">
        
        <div class="lg:col-span-8 space-y-6">
          <section class="bg-white p-6 rounded-2xl shadow-sm border border-slate-200">
            <h2 class="text-xl font-bold mb-4 flex items-center gap-2">
              <span>🔍</span> Nouveau Scan
            </h2>
            <ScanButton />
          </section>

          <AiSummary 
           date="12/01" 
           status="VALIDE" 
           note="Votre taux de fer est légèrement bas, pensez à manger des épinards !" 
          />
        </div>

        <aside class="lg:col-span-4">
          <div class="bg-white p-6 rounded-2xl shadow-sm border border-slate-200 h-full">
            <h2 class="text-xl font-bold mb-6 border-b pb-2 uppercase tracking-wide text-slate-700 italic">
              Historique
            </h2>
            <HistoryList :items="historiqueData" />
          </div>
        </aside>

      </div>
    </main>

    <footer class="mt-12 py-8 border-t border-slate-200 text-center text-slate-400 text-sm italic">
      Santé Scan — Outil d'aide à l'interprétation d'analyses médicales.
    </footer>
  </div>
</template>

<script setup>
import AppHeader from '../../components/AppHeader.vue'
import ScanButton from '../../components/ScanButton.vue'
import AiSummary from '../../components/AiSummary.vue'
import HistoryList from '../../components/HistoryList.vue'

const historiqueData = [
  { id: 1, date: '01/12/2025', titre: 'Bilan Complet', resultat: 'OK', couleur: 'text-green-600' },
  { id: 2, date: '15/09/2025', titre: 'Vitamines', resultat: 'Parfait', couleur: 'text-green-600' },
  { id: 3, date: '03/05/2025', titre: 'Bilan Standard', resultat: 'À vérifier', couleur: 'text-orange-500' },
]
</script>