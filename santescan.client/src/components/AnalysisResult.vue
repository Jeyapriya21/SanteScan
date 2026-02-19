<template>
  <div v-if="result" class="w-full max-w-4xl mx-auto">
    <!-- Carte de résultat - Style de votre maquette + fonctionnalités complètes -->
    <div class="bg-[#E8F2FF] rounded-2xl p-8 shadow-md border-l-8 border-green-500 relative">
      <!-- Petit losange vert décoratif (comme votre maquette) -->
      <div class="absolute -right-4 top-1/2 transform -translate-y-1/2 w-8 h-8 bg-green-500 rotate-45"></div>

      <!-- Header avec icône -->
      <div class="flex items-center gap-4 mb-6">
        <div class="w-14 h-14 bg-white rounded-xl flex items-center justify-center shadow-sm">
          <span class="text-3xl">📊</span>
        </div>
        <div>
          <p class="text-xs text-gray-600 uppercase tracking-wider font-semibold">
            Analyse du {{ formatDate(result.uploadDate) }}
          </p>
          <h2 class="text-2xl font-bold text-gray-800 mt-1">
            Votre bilan est
            <span class="underline decoration-2 decoration-green-600"
                  :class="getStatusColor(result.status)">
              {{ result.status.toUpperCase() }}
            </span>
          </h2>
        </div>
      </div>

      <!-- ✨ NOUVEAU : Résumé IA structuré et visuel -->
      <div v-if="analysisDetails" class="mb-6 space-y-4">
        <!-- Résumé général -->
        <div class="bg-white rounded-xl p-6 shadow-sm">
          <div class="flex items-start gap-3 mb-4">
            <span class="text-3xl">🩺</span>
            <div>
              <h3 class="text-lg font-bold text-gray-800 mb-2">Résumé de votre bilan</h3>
              <div class="prose prose-sm max-w-none">
                <p class="text-gray-700 leading-relaxed" v-html="formatSummary(analysisDetails.aiSummary)"></p>
              </div>
            </div>
          </div>
        </div>

        <!-- ✨ Parsing intelligent du résumé IA -->
        <div v-if="parsedResults.length > 0" class="space-y-3">
          <h3 class="text-sm font-bold text-gray-700 uppercase tracking-wider">Détails de l'analyse</h3>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
            <div v-for="(item, index) in parsedResults"
                 :key="index"
                 class="bg-white rounded-xl p-4 shadow-sm border-l-4 transition-all hover:shadow-md"
                 :class="getResultCardClass(item.status)">
              <div class="flex items-start gap-3">
                <span class="text-2xl">{{ getResultIcon(item.status) }}</span>
                <div class="flex-1">
                  <p class="font-semibold text-gray-800 text-sm">{{ item.parameter }}</p>
                  <p class="text-xs text-gray-600 mt-1">{{ item.value }}</p>
                  <p v-if="item.note" class="text-xs text-gray-500 mt-2 italic">
                    💡 {{ item.note }}
                  </p>
                </div>
                <span class="text-xs font-bold px-2 py-1 rounded-full"
                      :class="getStatusBadgeClass(item.status)">
                  {{ item.status }}
                </span>
              </div>
            </div>
          </div>
        </div>

        <!-- Message de synthèse personnalisé -->
        <div class="bg-gradient-to-r from-blue-50 to-purple-50 rounded-xl p-5 border border-blue-200">
          <div class="flex items-start gap-3">
            <span class="text-2xl">💬</span>
            <div>
              <p class="font-semibold text-gray-800 mb-2">Recommandation générale</p>
              <p class="text-sm text-gray-700 leading-relaxed">
                {{ getGeneralRecommendation(analysisDetails.aiSummary) }}
              </p>
            </div>
          </div>
        </div>
      </div>

      <!-- Loader pendant le chargement des détails -->
      <div v-else-if="loading" class="mb-6">
        <div class="bg-white/80 backdrop-blur-sm p-6 rounded-xl">
          <p class="text-gray-500 italic flex items-center gap-3">
            <span class="animate-spin text-2xl">⏳</span>
            Analyse en cours par l'intelligence artificielle...
          </p>
        </div>
      </div>

      <!-- Badge IA (comme votre maquette) -->
      <div class="flex items-center gap-2 mb-6">
        <span class="w-2.5 h-2.5 bg-green-500 rounded-full animate-pulse"></span>
        <p class="text-xs text-gray-600 uppercase tracking-wider font-semibold">
          Interprété par Santé Scan AI
        </p>
      </div>

      <!-- Disclaimer médical - Plus visible -->
      <div v-if="analysisDetails" class="bg-gradient-to-r from-red-50 to-orange-50 border-l-4 border-red-500 p-5 rounded-lg mb-6 shadow-sm">
        <div class="flex items-start gap-3">
          <span class="text-2xl">⚠️</span>
          <div>
            <p class="font-bold text-red-900 mb-1">Important</p>
            <p class="text-sm text-red-800 leading-relaxed">
              {{ analysisDetails.medicalDisclaimer }}
            </p>
          </div>
        </div>
      </div>

      <!-- Mode invité -->
      <div v-if="result.isGuest" class="bg-yellow-50 border-l-4 border-yellow-500 p-5 rounded-lg mb-6">
        <div class="flex items-start gap-3">
          <span class="text-2xl">💡</span>
          <div>
            <p class="font-semibold text-yellow-900 mb-1">Mode Invité</p>
            <p class="text-sm text-yellow-800">
              Créez un compte pour sauvegarder vos analyses et suivre l'évolution de votre santé dans le temps.
            </p>
            <button @click="$emit('show-register')"
                    class="mt-3 text-sm font-semibold text-yellow-900 underline hover:text-yellow-700">
              Créer mon compte maintenant →
            </button>
          </div>
        </div>
      </div>

      <!-- Statistiques rapides -->
      <div v-if="analysisDetails" class="grid grid-cols-3 gap-4 mb-6">
        <div class="bg-white/60 p-4 rounded-xl text-center">
          <p class="text-2xl font-bold text-gray-800">{{ getAnalysisCount() }}</p>
          <p class="text-xs text-gray-600 uppercase tracking-wide">Analyses</p>
        </div>
        <div class="bg-white/60 p-4 rounded-xl text-center">
          <p class="text-2xl font-bold text-green-600">{{ result.status }}</p>
          <p class="text-xs text-gray-600 uppercase tracking-wide">Statut</p>
        </div>
        <div class="bg-white/60 p-4 rounded-xl text-center">
          <p class="text-2xl font-bold text-blue-600">IA</p>
          <p class="text-xs text-gray-600 uppercase tracking-wide">Analysé</p>
        </div>
      </div>

      <!-- Boutons d'action -->
      <div class="grid grid-cols-2 gap-4">
        <button v-if="analysisDetails"
                @click="downloadReport"
                class="bg-white hover:bg-gray-50 text-gray-800 font-semibold py-4 px-6 rounded-xl transition-all duration-200 flex items-center justify-center gap-3 border-2 border-gray-200 hover:border-blue-400">
          <span class="text-xl">📄</span>
          <span class="uppercase tracking-wide text-sm">Télécharger</span>
        </button>

        <button @click="newAnalysis"
                class="bg-[#0066FF] hover:bg-[#0052CC] text-white font-bold py-4 px-6 rounded-xl transition-all duration-200 flex items-center justify-center gap-3 shadow-lg hover:shadow-xl"
                :class="{ 'col-span-2': !analysisDetails }">
          <span class="text-xl">📷</span>
          <span class="uppercase tracking-wide text-sm">Nouvelle Analyse</span>
        </button>
      </div>

      <!-- Texte brut (toggle) -->
      <div v-if="analysisDetails && showRawText" class="mt-6">
        <button @click="showRawText = false"
                class="text-sm text-gray-500 underline mb-2">
          Masquer le texte brut
        </button>
        <div class="bg-gray-100 p-4 rounded-lg max-h-60 overflow-y-auto">
          <pre class="text-xs text-gray-700 whitespace-pre-wrap">{{ analysisDetails.rawText }}</pre>
        </div>
      </div>
      <button v-else-if="analysisDetails"
              @click="showRawText = true"
              class="mt-4 text-sm text-gray-500 underline hover:text-gray-700">
        Voir le texte extrait (OCR)
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted, computed } from 'vue';

  interface AnalysisResult {
    analysisId: string;
    message: string;
    uploadDate?: string;
    status: string;
    isGuest: boolean;
  }

  interface AnalysisDetails {
    id: string;
    userId: string;
    uploadDate: string;
    rawText: string;
    aiSummary: string;
    globalStatus: string;
    medicalDisclaimer: string;
    details: any[];
  }

  interface ParsedResult {
    parameter: string;
    value: string;
    status: 'OK' | 'Attention' | 'Critique' | 'Info';
    note?: string;
  }

  const props = defineProps<{
    result: AnalysisResult | null;
  }>();

  const emit = defineEmits<{
    (e: 'new-analysis'): void;
    (e: 'show-register'): void;
  }>();

  const analysisDetails = ref<AnalysisDetails | null>(null);
  const loading = ref(false);
  const showRawText = ref(false);

  // ✨ Parser intelligent du résumé IA
  const parsedResults = computed<ParsedResult[]>(() => {
    if (!analysisDetails.value?.aiSummary) return [];

    const summary = analysisDetails.value.aiSummary;
    const results: ParsedResult[] = [];

    // Recherche de patterns dans le texte IA
    // Exemples : "Hémoglobine : 14 g/L (normal)", "Glycémie légèrement élevée", etc.

    // Pattern 1: Paramètre : Valeur (statut)
    const pattern1 = /([A-Za-zÀ-ÿ\s]+)\s*:\s*([0-9.,]+\s*[a-zA-Z\/]+)\s*\(([^)]+)\)/gi;
    const matchesArray = Array.from(summary.matchAll(pattern1));

    matchesArray.forEach(match => {
      if (match && match.length >= 4) {
        // Safe access
      }
    });

    // Pattern 2: Recherche de mots-clés courants
    const keywords = [
      { pattern: /hémoglobine|globule rouge/i, name: 'Hémoglobine' },
      { pattern: /glycémie|glucose/i, name: 'Glycémie' },
      { pattern: /cholestérol/i, name: 'Cholestérol' },
      { pattern: /vitamine/i, name: 'Vitamines' },
      { pattern: /fer|ferritine/i, name: 'Fer' }
    ];

    keywords.forEach(keyword => {
      if (keyword.pattern.test(summary) && !results.some(r => r.parameter.toLowerCase().includes(keyword.name.toLowerCase()))) {
        const context = extractContextAroundKeyword(summary, keyword.pattern);
        if (context) {
          results.push({
            parameter: keyword.name,
            value: context.value || 'Voir résumé',
            status: context.status,
            note: context.note
          });
        }
      }
    });

    return results;
  });

  onMounted(() => {
    if (props.result?.analysisId) {
      fetchAnalysisDetails();
    }
  });

  const fetchAnalysisDetails = async () => {
    if (!props.result?.analysisId) return;

    loading.value = true;

    try {
      const sessionId = localStorage.getItem('santescan_session_id');
      const response = await fetch(`/api/Analyses/${props.result.analysisId}`, {
        headers: {
          'X-Session-Id': sessionId || '',
        },
      });

      if (response.ok) {
        analysisDetails.value = await response.json();
        console.log('✅ Détails récupérés:', analysisDetails.value);
      } else {
        console.error('❌ Erreur lors de la récupération des détails');
      }
    } catch (err) {
      console.error('❌ Erreur réseau:', err);
    } finally {
      loading.value = false;
    }
  };

  // ✨ Formater le résumé avec mise en forme HTML
  const formatSummary = (summary: string): string => {
    if (!summary) return '';

    return summary
      .replace(/\*\*([^*]+)\*\*/g, '<strong class="text-gray-900">$1</strong>') // Gras
      .replace(/\*([^*]+)\*/g, '<em>$1</em>') // Italique
      .replace(/\n/g, '<br>') // Retours à la ligne
      .replace(/(normal|bon|correct|ok)/gi, '<span class="text-green-600 font-semibold">$1</span>') // Vert pour "normal"
      .replace(/(élevé|bas|attention|surveiller)/gi, '<span class="text-orange-600 font-semibold">$1</span>'); // Orange pour "attention"
  };

  // Détecter le statut depuis le texte
  const getStatusFromText = (text: string): 'OK' | 'Attention' | 'Critique' | 'Info' => {
    const lowerText = text.toLowerCase();
    if (lowerText.includes('normal') || lowerText.includes('bon') || lowerText.includes('correct')) {
      return 'OK';
    } else if (lowerText.includes('élevé') || lowerText.includes('bas') || lowerText.includes('attention')) {
      return 'Attention';
    } else if (lowerText.includes('critique') || lowerText.includes('urgent')) {
      return 'Critique';
    }
    return 'Info';
  };

  // Extraire le contexte autour d'un mot-clé
  const extractContextAroundKeyword = (text: string, pattern: RegExp): { value: string; status: 'OK' | 'Attention' | 'Critique' | 'Info'; note?: string } | null => {
    const match = pattern.exec(text);
    if (!match) return null;

    const index = match.index;
    const contextLength = 100;
    const context = text.substring(Math.max(0, index - contextLength), Math.min(text.length, index + contextLength));

    return {
      value: 'Mentionné dans l\'analyse',
      status: getStatusFromText(context),
      note: context.length < 150 ? context : context.substring(0, 147) + '...'
    };
  };

  // Extraire une note depuis le contexte
  const extractNoteFromContext = (text: string, index: number): string | undefined => {
    const sentence = text.substring(index, text.indexOf('.', index) + 1);
    return sentence.length > 20 && sentence.length < 200 ? sentence : undefined;
  };

  // Obtenir la recommandation générale
  const getGeneralRecommendation = (summary: string): string => {
    if (!summary) return 'Consultez votre médecin pour interpréter ces résultats.';

    const lowerSummary = summary.toLowerCase();

    if (lowerSummary.includes('consulter') || lowerSummary.includes('médecin')) {
      const startIndex = summary.toLowerCase().indexOf('consulter');
      if (startIndex !== -1) {
        const recommendation = summary.substring(startIndex);
        return recommendation.substring(0, recommendation.indexOf('.') + 1) || recommendation;
      }
    }

    if (lowerSummary.includes('épinard') || lowerSummary.includes('alimentation') || lowerSummary.includes('manger')) {
      return 'Privilégiez une alimentation équilibrée riche en fer et en vitamines.';
    }

    return 'Continuez à surveiller votre santé et consultez votre médecin si nécessaire.';
  };

  const formatDate = (dateString?: string): string => {
    if (!dateString) return new Date().toLocaleDateString('fr-FR');
    return new Date(dateString).toLocaleDateString('fr-FR', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    }).replace('.', '');
  };

  const getStatusColor = (status: string): string => {
    const statusLower = status.toLowerCase();
    if (statusLower.includes('ok') || statusLower.includes('valide') || statusLower.includes('normal')) {
      return 'text-green-600';
    } else if (statusLower.includes('surveiller') || statusLower.includes('attention')) {
      return 'text-orange-600';
    } else if (statusLower.includes('critique') || statusLower.includes('urgent')) {
      return 'text-red-600';
    }
    return 'text-blue-600';
  };

  const getResultCardClass = (status: string): string => {
    switch (status) {
      case 'OK':
        return 'border-green-500 bg-green-50/50';
      case 'Attention':
        return 'border-orange-500 bg-orange-50/50';
      case 'Critique':
        return 'border-red-500 bg-red-50/50';
      default:
        return 'border-blue-500 bg-blue-50/50';
    }
  };

  const getResultIcon = (status: string): string => {
    switch (status) {
      case 'OK':
        return '✅';
      case 'Attention':
        return '⚠️';
      case 'Critique':
        return '🚨';
      default:
        return 'ℹ️';
    }
  };

  const getStatusBadgeClass = (status: string): string => {
    switch (status) {
      case 'OK':
        return 'bg-green-100 text-green-800';
      case 'Attention':
        return 'bg-orange-100 text-orange-800';
      case 'Critique':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-blue-100 text-blue-800';
    }
  };

  const getAnalysisCount = (): number => {
    const count = localStorage.getItem('santescan_analyses_count');
    return count ? parseInt(count, 10) : 1;
  };

  const downloadReport = () => {
    if (!analysisDetails.value) return;

    const report = `
SANTÉ SCAN - RAPPORT D'ANALYSE
================================

Date: ${formatDate(props.result?.uploadDate)}
Statut: ${props.result?.status}

RÉSUMÉ IA:
${analysisDetails.value.aiSummary}

DISCLAIMER:
${analysisDetails.value.medicalDisclaimer}

TEXTE BRUT EXTRAIT (OCR):
${analysisDetails.value.rawText}
  `.trim();

    const blob = new Blob([report], { type: 'text/plain' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `santescan_analyse_${props.result?.analysisId}.txt`;
    a.click();
    window.URL.revokeObjectURL(url);

    console.log('📄 Rapport téléchargé');
  };

  const newAnalysis = () => {
    emit('new-analysis');
  };
</script>
