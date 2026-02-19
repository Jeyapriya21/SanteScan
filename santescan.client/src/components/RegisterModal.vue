<template>
  <div v-if="show" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
    <div class="bg-white rounded-3xl p-8 max-w-md w-full shadow-2xl">
      <h2 class="text-3xl font-bold text-center mb-6">
        🎉 Créez votre compte
      </h2>
      
      <p class="text-center text-slate-600 mb-6">
        Sauvegardez vos analyses et suivez votre santé !
      </p>

      <form @submit.prevent="register">
        <div class="space-y-4">
          <input
            v-model="email"
            type="email"
            placeholder="Email"
            required
            class="w-full px-4 py-3 border-2 border-slate-300 rounded-xl focus:border-blue-500 outline-none"
          />
          
          <input
            v-model="password"
            type="password"
            placeholder="Mot de passe"
            required
            minlength="6"
            class="w-full px-4 py-3 border-2 border-slate-300 rounded-xl focus:border-blue-500 outline-none"
          />
          
          <input
            v-model.number="age"
            type="number"
            placeholder="Âge"
            required
            min="1"
            max="150"
            class="w-full px-4 py-3 border-2 border-slate-300 rounded-xl focus:border-blue-500 outline-none"
          />
          
          <select
            v-model="gender"
            class="w-full px-4 py-3 border-2 border-slate-300 rounded-xl focus:border-blue-500 outline-none"
          >
            <option value="">Genre (optionnel)</option>
            <option value="Homme">Homme</option>
            <option value="Femme">Femme</option>
            <option value="Autre">Autre</option>
          </select>
        </div>

        <p v-if="error" class="text-red-500 text-sm mt-4 text-center">
          {{ error }}
        </p>

        <div class="flex gap-4 mt-6">
          <button
            type="button"
            @click="$emit('close')"
            class="flex-1 py-3 bg-slate-200 text-slate-700 font-bold rounded-xl hover:bg-slate-300"
          >
            Plus tard
          </button>
          
          <button
            type="submit"
            :disabled="isRegistering"
            class="flex-1 py-3 bg-sky-400 text-slate-900 font-bold rounded-xl hover:bg-sky-500 disabled:opacity-50"
          >
            {{ isRegistering ? '⏳ Création...' : '✅ Créer' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';

defineProps<{
  show: boolean;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'registered', userId: string): void;
}>();

const email = ref('');
const password = ref('');
const age = ref<number>();
const gender = ref('');
const error = ref('');
const isRegistering = ref(false);

const register = async () => {
  error.value = '';
  isRegistering.value = true;

  const sessionId = localStorage.getItem('santescan_session_id');

  try {
    const response = await fetch('/api/Auth/register', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-Session-Id': sessionId || '',
      },
      body: JSON.stringify({
        email: email.value,
        password: password.value,
        age: age.value,
        gender: gender.value || null,
      }),
    });

    if (!response.ok) {
      const errorData = await response.json();
      error.value = errorData.message || 'Erreur lors de la création du compte';
      return;
    }

    const data = await response.json();
    console.log('✅ Compte créé:', data);
    
    // Supprimer la session guest
    localStorage.removeItem('santescan_session_id');
    
    alert(`✅ Compte créé ! ${data.analysesConservees || 0} analyse(s) conservée(s).`);
    emit('registered', data.userId);
    emit('close');

  } catch (err) {
    console.error('❌ Erreur:', err);
    error.value = 'Erreur réseau. Réessayez.';
  } finally {
    isRegistering.value = false;
  }
};
</script>