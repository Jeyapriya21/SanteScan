<template>
  <div class="min-h-screen bg-gray-50">
    <!-- Header (à créer selon votre maquette) -->
    <header class="bg-white shadow-sm border-b border-gray-200">
      <div class="container mx-auto px-6 py-4 flex items-center justify-between">
        <!-- Logo -->
        <div class="flex items-center gap-3">
          <div class="w-12 h-12 bg-blue-500 rounded-full flex items-center justify-center">
            <span class="text-white text-2xl">🩺</span>
          </div>
          <div>
            <h1 class="text-2xl font-bold text-gray-800">SANTÉ SCAN</h1>
            <p class="text-xs text-blue-500 uppercase tracking-wide">Information médicale</p>
          </div>
        </div>

        <!-- Navigation -->
        <nav class="flex gap-8">
          <a href="#" class="text-blue-500 font-semibold border-b-2 border-blue-500 pb-1">Accueil</a>
          <a href="#" class="text-gray-600 hover:text-blue-500">Historique</a>
          <a href="#" class="text-gray-600 hover:text-blue-500">Conseils</a>
        </nav>

        <!-- User profile -->
        <div class="flex items-center gap-3">
          <div class="text-right">
            <p class="font-semibold text-gray-800">Utilisateur</p>
            <p class="text-xs text-gray-500">Mon Profil</p>
          </div>
          <div class="w-10 h-10 bg-blue-500 rounded-full flex items-center justify-center">
            <span class="text-white">👤</span>
          </div>
        </div>
      </div>
    </header>

    <!-- Main content avec layout 2 colonnes -->
    <div class="container mx-auto px-6 py-8">
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <!-- Colonne principale (2/3) -->
        <div class="lg:col-span-2 space-y-6">
          <!-- Zone de scan -->
          <div class="bg-white rounded-2xl p-8 shadow-sm border border-gray-200">
            <div class="flex items-center gap-3 mb-6">
              <span class="text-2xl">🔍</span>
              <h2 class="text-2xl font-bold text-gray-800">Nouveau Scan</h2>
            </div>

            <ScanButton v-if="!analysisResult"
                        @analysis-finished="handleAnalysisFinished" />
          </div>

          <!-- Résultat de l'analyse -->
          <AnalysisResult v-if="analysisResult"
                          :result="analysisResult"
                          @new-analysis="resetAnalysis"
                          @show-register="showRegisterModal = true" />
        </div>

        <!-- Sidebar historique (1/3) -->
        <div class="lg:col-span-1">
          <HistoryView :refresh-trigger="historyRefreshTrigger"
                        @select="viewAnalysisDetails"
                        @view-all="goToFullHistory" />
        </div>
      </div>
    </div>

    <!-- Modal d'inscription -->
    <RegisterModal v-if="showRegisterModal"
                   :show="showRegisterModal"
                   @close="showRegisterModal = false"
                   @registered="handleRegistered" />
  </div>
</template>

<script setup lang="ts">
  import { ref } from 'vue';
  import ScanButton from '@/components/ScanButton.vue';
  import AnalysisResult from '@/components/AnalysisResult.vue';
  import HistoryView from '@/components/HistoryView.vue';

  interface AnalysisResult {
    analysisId: string;
    message: string;
    uploadDate?: string;
    status: string;
    isGuest: boolean;
  }

  const analysisResult = ref<AnalysisResult | null>(null);
  const showRegisterModal = ref(false);
  const historyRefreshTrigger = ref(0);

  const handleAnalysisFinished = (result: AnalysisResult) => {
    analysisResult.value = result;
    // Rafraîchir l'historique après une nouvelle analyse
    historyRefreshTrigger.value++;

    // Incrémenter le compteur d'analyses
    const currentCount = parseInt(localStorage.getItem('santescan_analyses_count') || '0');
    localStorage.setItem('santescan_analyses_count', (currentCount + 1).toString());
  };

  const resetAnalysis = () => {
    analysisResult.value = null;
  };

  const handleRegistered = (userId: string) => {
    console.log('✅ Utilisateur inscrit:', userId);
    showRegisterModal.value = false;
    // Rafraîchir l'historique après inscription (les analyses ont été migrées)
    historyRefreshTrigger.value++;
  };

  const viewAnalysisDetails = async (analysisId: string) => {
    console.log('📊 Chargement de l\'analyse:', analysisId);

    // Charger les détails de l'analyse sélectionnée
    try {
      const sessionId = localStorage.getItem('santescan_session_id');
      const response = await fetch(`/api/Analyses/${analysisId}`, {
        headers: {
          'X-Session-Id': sessionId || '',
        },
      });

      if (response.ok) {
        const details = await response.json();

        // Afficher dans le résultat (réutiliser AnalysisResult)
        analysisResult.value = {
          analysisId: details.id,
          message: 'Analyse récupérée',
          uploadDate: details.uploadDate,
          status: details.globalStatus,
          isGuest: !!details.sessionId,
        };

        console.log('✅ Détails chargés');
      }
    } catch (err) {
      console.error('❌ Erreur:', err);
    }
  };

  const goToFullHistory = () => {
    console.log('📜 Naviguer vers l\'historique complet');
    // TODO: Router.push('/history')
  };
</script>
