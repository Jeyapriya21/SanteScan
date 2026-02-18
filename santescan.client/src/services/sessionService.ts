import { v4 as uuidv4 } from 'uuid';

const SESSION_KEY = 'santescan_session_id';
const SESSION_ANALYSES_COUNT_KEY = 'santescan_analyses_count';

/**
 * Service de gestion des sessions pour les utilisateurs guests
 */
export class SessionService {
  /**
   * Obtient le sessionId existant ou en crée un nouveau
   */
  static getOrCreateSessionId(): string {
    let sessionId = localStorage.getItem(SESSION_KEY);

    if (!sessionId) {
      sessionId = uuidv4();
      localStorage.setItem(SESSION_KEY, sessionId); // ✅ CORRECTION : setItem au lieu de getItem
      console.log('✅ Nouvelle session guest créée:', sessionId);
    } else {
      console.log('📌 Session existante:', sessionId);
    }

    return sessionId;
  }

  /**
   * Vérifie si une session existe
   */
  static hasSession(): boolean {
    return !!localStorage.getItem(SESSION_KEY);
  }

  /**
   * Récupère le sessionId actuel (ou null si n'existe pas)
   */
  static getSessionId(): string | null {
    return localStorage.getItem(SESSION_KEY);
  }

  /**
   * Supprime la session (à appeler après inscription réussie)
   */
  static clearSession(): void {
    const sessionId = localStorage.getItem(SESSION_KEY);
    localStorage.removeItem(SESSION_KEY);
    localStorage.removeItem(SESSION_ANALYSES_COUNT_KEY);
    console.log('🗑️ Session supprimée:', sessionId);
  }

  /**
   * Incrémente le compteur d'analyses pour cette session
   */
  static incrementAnalysisCount(): number {
    const currentCount = this.getAnalysisCount();
    const newCount = currentCount + 1;
    localStorage.setItem(SESSION_ANALYSES_COUNT_KEY, newCount.toString());
    console.log('📊 Analyses dans cette session:', newCount);
    return newCount;
  }

  /**
   * Récupère le nombre d'analyses effectuées dans cette session
   */
  static getAnalysisCount(): number {
    const count = localStorage.getItem(SESSION_ANALYSES_COUNT_KEY);
    return count ? parseInt(count, 10) : 0;
  }

  /**
   * Réinitialise le compteur d'analyses
   */
  static resetAnalysisCount(): void {
    localStorage.setItem(SESSION_ANALYSES_COUNT_KEY, '0');
  }
}
