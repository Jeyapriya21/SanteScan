<template>
  <div class="bg-white rounded-2xl p-6 shadow-sm border border-gray-200 h-fit">
    <!-- Header -->
    <h2 class="text-xl font-bold text-gray-800 mb-1">HISTORIQUE</h2>
    <p class="text-xs text-gray-500 uppercase tracking-wider mb-6">Derniers scans</p>

    <!-- Loading state -->
    <div v-if="loading" class="text-center py-8">
      <span class="text-2xl animate-spin inline-block">⏳</span>
      <p class="text-sm text-gray-500 mt-2">Chargement...</p>
    </div>

    <!-- Empty state -->
    <div v-else-if="analyses.length === 0" class="text-center py-8">
      <span class="text-4xl opacity-50">📭</span>
      <p class="text-sm text-gray-500 mt-2">Aucune analyse pour le moment</p>
    </div>

    <!-- Liste des analyses -->
    <div v-else class="space-y-4">
      <div 
        v-for="analysis in analyses" 
        :key="analysis.id"
        class="border-b border-gray-200 pb-4 last:border-b-0 cursor-pointer hover:bg-gray-50 p-2 rounded-lg transition-colors"
        @click="selectAnalysis(analysis.id)"
      >
        <div class="flex justify-between items-start">
          <div class="flex-1">
            <p class="font-semibold text-gray-800 text-sm">
              {{ getAnalysisTitle(analysis) }}
            </p>
            <p class="text-xs text-gray-500 mt-1">
              {{ formatDate(analysis.uploadDate) }}
            </p>
          </div>
          <span 
            class="text-xs font-semibold px-3 py-1 rounded-full"
            :class="getStatusBadgeClass(analysis.globalStatus)"
          >
            {{ analysis.globalStatus }}
          </span>
        </div>
        
        <!-- Résumé court (optionnel) -->
        <p v-if="analysis.aiSummary" class="text-xs text-gray-600 mt-2 line-clamp-2">
          {{ getTruncatedSummary(analysis.aiSummary) }}
        </p>
      </div>
    </div>

    <!-- Bouton voir tout -->
    <button 
      v-if="analyses.length > 0"
      @click="$emit('view-all')"
      class="w-full mt-4 text-sm text-blue-600 hover:text-blue-800 font-semibold underline"
    >
      Voir tout l'historique →
    </button>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';

interface Analysis {
  id: string;
  uploadDate: string;
  globalStatus: string;
  aiSummary: string;
}

const props = defineProps<{
  refreshTrigger?: number; // Pour rafraîchir quand une nouvelle analyse est ajoutée
}>();

const emit = defineEmits<{
  (e: 'select', analysisId: string): void;
  (e: 'view-all'): void;
}>();

const analyses = ref<Analysis[]>([]);
const loading = ref(false);

onMounted(() => {
  fetchHistory();
});

// Rafraîchir quand une nouvelle analyse est créée
watch(() => props.refreshTrigger, () => {
  if (props.refreshTrigger) {
    fetchHistory();
  }
});

const fetchHistory = async () => {
  loading.value = true;
  
  try {
    const sessionId = localStorage.getItem('santescan_session_id');
    
    if (!sessionId) {
      console.log('ℹ️ Pas de session, historique vide');
      analyses.value = [];
      return;
    }

    const response = await fetch(`/api/Analyses/session/${sessionId}`);
    
    if (response.ok) {
      const data = await response.json();
      analyses.value = data.analyses || [];
      console.log('✅ Historique chargé:', analyses.value.length, 'analyses');
    } else {
      console.error('❌ Erreur lors du chargement de l\'historique');
      analyses.value = [];
    }
  } catch (err) {
    console.error('❌ Erreur réseau:', err);
    analyses.value = [];
  } finally {
    loading.value = false;
  }
};

const formatDate = (dateString: string): string => {
  const date = new Date(dateString);
  const day = date.getDate().toString().padStart(2, '0');
  const monthNames = ['jan', 'fev', 'mar', 'avr', 'mai', 'juin', 'juil', 'août', 'sep', 'oct', 'nov', 'dec'];
  const month = monthNames[date.getMonth()];
  const year = date.getFullYear();
  const hours = date.getHours().toString().padStart(2, '0');
  const minutes = date.getMinutes().toString().padStart(2, '0');
  
  return `${day} ${month} ${year} - ${hours}:${minutes}`;
};

const getAnalysisTitle = (analysis: Analysis): string => {
  // Vous pouvez personnaliser selon le type d'analyse
  return 'Bilan sanguin';
};

const getStatusBadgeClass = (status: string): string => {
  const statusLower = status.toLowerCase();
  
  if (statusLower.includes('ok') || statusLower.includes('normal') || statusLower.includes('valide')) {
    return 'bg-green-100 text-green-800';
  } else if (statusLower.includes('surveiller') || statusLower.includes('attention')) {
    return 'bg-orange-100 text-orange-800';
  } else if (statusLower.includes('critique') || statusLower.includes('urgent')) {
    return 'bg-red-100 text-red-800';
  }
  
  return 'bg-blue-100 text-blue-800';
};

const getTruncatedSummary = (summary: string): string => {
  if (summary.length <= 80) return summary;
  return summary.substring(0, 77) + '...';
};

const selectAnalysis = (id: string) => {
  emit('select', id);
  console.log('📊 Analyse sélectionnée:', id);
};
</script>