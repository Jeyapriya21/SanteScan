<template>
  <div class="w-full">
    <input type="file"
           ref="fileInput"
           @change="handleFileUpload"
           accept="image/*,.pdf"
           class="hidden" />

    <section @click="triggerFileInput"
             @dragover.prevent="onDragOver"
             @dragleave.prevent="onDragLeave"
             @drop.prevent="onDrop"
             :class="[
        'relative cursor-pointer border-2 border-dashed rounded-3xl p-10 transition-all duration-300 flex flex-col items-center justify-center gap-6 group',
        isDragging
          ? 'border-blue-500 bg-blue-50 scale-[1.01] shadow-xl'
          : 'border-slate-300 bg-white hover:border-blue-400 hover:bg-slate-50'
      ]">
      <div v-if="!isUploading" class="text-5xl group-hover:scale-110 transition-transform duration-300">
        {{ isDragging ? '📂' : '📄' }}
      </div>

      <div class="w-full max-w-xs bg-sky-400 text-slate-900 font-bold py-4 rounded-xl shadow-[0_4px_0_0_rgba(14,165,233,1)] border-2 border-slate-800 flex items-center justify-center gap-3 group-hover:bg-sky-500">
        <span v-if="!isUploading" class="text-2xl">📷</span>
        <span v-else class="animate-spin text-2xl">⏳</span>
        <span class="text-lg uppercase italic tracking-wide">
          {{ isUploading ? 'Analyse IA...' : 'Scanner mon bilan' }}
        </span>
      </div>

      <div class="text-center">
        <p class="text-slate-500 font-bold italic">
          {{ isDragging ? 'Relâchez pour analyser' : 'Cliquez ou glissez votre bilan ici' }}
        </p>
      </div>
    </section>

    <p v-if="uploadError" class="text-center text-sm text-red-500 mt-4 font-bold p-2 bg-red-50 rounded-lg">
      ⚠️ {{ uploadError }}
    </p>
  </div>
</template>

<script setup lang="ts">
  import { ref } from 'vue';

  // 1. Interface pour la réponse du backend ASP.NET Core
  interface AnalysisResult {
    analysisId: string;
    message: string;
    uploadDate: string;
    status: string;
  }

  // 2. Interface pour les détails de l'analyse (si vous voulez récupérer plus d'infos)
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

  // 3. Définition des événements que le composant peut envoyer
  const emit = defineEmits<{
    (e: 'analysis-finished', result: AnalysisResult): void;
  }>();

  const fileInput = ref<HTMLInputElement | null>(null);
  const isUploading = ref<boolean>(false);
  const isDragging = ref<boolean>(false);
  const uploadError = ref<string | null>(null);

  // ✅ Configuration de l'API (à adapter selon votre environnement)
  const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:7250';

  const triggerFileInput = (): void => {
    fileInput.value?.click();
  };

  const onDragOver = (): void => { isDragging.value = true; };
  const onDragLeave = (): void => { isDragging.value = false; };

  const onDrop = (event: DragEvent): void => {
    isDragging.value = false;
    const file = event.dataTransfer?.files[0];
    if (file) processFile(file);
  };

  const handleFileUpload = (event: Event): void => {
    const target = event.target as HTMLInputElement;
    const file = target.files?.[0];
    if (file) processFile(file);
  };

  const processFile = async (file: File): Promise<void> => {
    // ✅ Validation des formats (correspondant au backend)
    const validTypes: string[] = ['image/jpeg', 'image/jpg', 'image/png', 'application/pdf'];
    const validExtensions: string[] = ['.jpg', '.jpeg', '.png', '.pdf'];
    const fileExtension = '.' + file.name.split('.').pop()?.toLowerCase();

    if (!validTypes.includes(file.type) || !validExtensions.includes(fileExtension)) {
      uploadError.value = "Format non supporté. Formats acceptés : JPG, PNG, PDF";
      return;
    }

    // ✅ Validation de la taille (10 MB max comme dans le backend)
    const maxSize = 10 * 1024 * 1024; // 10 MB
    if (file.size > maxSize) {
      uploadError.value = "Fichier trop volumineux (max 10 MB)";
      return;
    }

    isUploading.value = true;
    uploadError.value = null;

    const formData = new FormData();
    formData.append('file', file);

    try {
      // ✅ Envoi vers votre backend ASP.NET Core
      const response = await fetch(`${API_BASE_URL}/api/Analyses/upload`, {
        method: 'POST',
        body: formData,
        // ⚠️ Si vous avez un système d'authentification, décommentez:
        // headers: {
        //   'Authorization': `Bearer ${localStorage.getItem('authToken')}`,
        // },
      });

      // ✅ Gestion des différents codes d'erreur
      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));

        switch (response.status) {
          case 400:
            uploadError.value = "Fichier invalide. Vérifiez le format et la taille.";
            break;
          case 401:
            uploadError.value = "Vous devez être connecté pour analyser un bilan.";
            break;
          case 422:
            uploadError.value = errorData.error || "Impossible d'extraire le texte. Vérifiez la qualité de l'image.";
            break;
          case 503:
            uploadError.value = errorData.error || "Service d'analyse IA temporairement indisponible.";
            break;
          default:
            uploadError.value = "Une erreur est survenue lors de l'analyse.";
        }
        return;
      }

      // ✅ Récupération du résultat
      const data: AnalysisResult = await response.json();

      console.log('✅ Analyse terminée:', data);

      // Émettre le résultat au composant parent
      emit('analysis-finished', data);

    } catch (err) {
      console.error('❌ Erreur réseau:', err);
      uploadError.value = err instanceof Error
        ? `Erreur réseau: ${err.message}`
        : "Impossible de contacter le serveur. Vérifiez que l'API est lancée.";
    } finally {
      isUploading.value = false;
      // Réinitialiser l'input file pour permettre le re-upload du même fichier
      if (fileInput.value) {
        fileInput.value.value = '';
      }
    }
  };
</script>
