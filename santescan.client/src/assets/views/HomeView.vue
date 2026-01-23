<template>
  <div class="flex-1 overflow-y-auto p-6 flex flex-col items-center">
    <div class="mb-10 text-center">
      <img src="@/assets/logo.png" alt="Logo" class="w-24 h-24 mx-auto mb-2" />
      <h1 class="text-3xl font-black text-gray-800 uppercase italic">Santé Scan</h1>
    </div>

    <ScanButton @scan="startOcrProcess" />

    <AiSummary 
      messagePrincipal="Votre bilan du 12/01 est OK."
      detailIA="Fer légèrement bas."
      statusColor="bg-green-500"
    />

    <div class="w-full">
      <h3 class="text-lg font-extrabold text-gray-700 mb-4 uppercase tracking-tighter">
        Historique des bilans
      </h3>
      <HistoryList :items="bilans" />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import ScanButton from '@/components/ScanButton.vue';
import AiSummary from '@/components/AiSummary.vue';
import HistoryList from '@/components/HistoryList.vue';

const bilans = ref([
  { date: '01/12/2025', label: 'Bilan Complet', status: 'OK (veel)', color: 'text-green-600' },
  { date: '15/09/2025', label: 'Bilan Vitamines', status: 'OK', color: 'text-green-600' },
  { date: '03/05/2025', label: 'Bilan Standard', status: 'À vérifier', color: 'text-orange-500' }
]);

const startOcrProcess = () => {
  console.log("Appel au Backend .NET Core pour l'OCR...");
};
</script>